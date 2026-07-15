$(function () {
    let rxRowIndex = 0;

    function addRxRow(data) {
        data = data || {};
        const idx = rxRowIndex++;
        const row = `
            <tr data-row-index="${idx}">
                <td><input type="text" class="form-control form-control-sm" name="Items[${idx}].MedicationName" value="${data.medicationName || ''}" required /></td>
                <td><input type="text" class="form-control form-control-sm" name="Items[${idx}].Dosage" value="${data.dosage || ''}" /></td>
                <td><input type="text" class="form-control form-control-sm" name="Items[${idx}].Frequency" value="${data.frequency || ''}" /></td>
                <td><input type="number" class="form-control form-control-sm" name="Items[${idx}].DurationDays" value="${data.durationDays || ''}" /></td>
                <td><input type="text" class="form-control form-control-sm" name="Items[${idx}].Instructions" value="${data.instructions || ''}" /></td>
                <td><button type="button" class="btn btn-sm btn-danger remove-rx-row">&times;</button></td>
            </tr>`;
        $('#rxItemsBody').append(row);
    }

    // Start with one blank row
    addRxRow();

    $('#addRxRowBtn').on('click', function () {
        addRxRow();
    });

    $(document).on('click', '.remove-rx-row', function () {
        if ($('#rxItemsBody tr').length <= 1) {
            toastr.warning('At least one medication row is required.');
            return;
        }
        $(this).closest('tr').remove();
    });

    // Save vitals + diagnosis
    $('#saveRecordBtn').on('click', function () {
        const formData = $('#recordForm').serialize();
        $.post('/Consultation/SaveRecord', formData, function (result) {
            if (result.success) {
                $('#recordStatus').html('<span class="text-success">Saved ✓</span>');
                toastr.success('Vitals and diagnosis saved.');
            } else {
                $('#recordStatus').html('<span class="text-danger">' + result.message + '</span>');
                toastr.error(result.message);
            }
        });
    });

    // Save prescription (master-detail — collect all rows into indexed form data)
    $('#savePrescriptionBtn').on('click', function () {
        const items = [];
        $('#rxItemsBody tr').each(function () {
            const row = $(this);
            items.push({
                MedicationName: row.find('input[name$=".MedicationName"]').val(),
                Dosage: row.find('input[name$=".Dosage"]').val(),
                Frequency: row.find('input[name$=".Frequency"]').val(),
                DurationDays: row.find('input[name$=".DurationDays"]').val() || null,
                Instructions: row.find('input[name$=".Instructions"]').val()
            });
        });

        if (items.length === 0 || !items[0].MedicationName) {
            toastr.error('Add at least one medication.');
            return;
        }

        const payload = {
            AppointmentId: $('#rxAppointmentId').val(),
            Notes: $('#rxNotes').val(),
            Items: items
        };

        $.ajax({
            url: '/Consultation/SavePrescription',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (result) {
                if (result.success) {
                    toastr.success('Prescription saved.');
                    setTimeout(function () { window.location.href = result.url; }, 800);
                } else {
                    $('#rxStatus').html('<span class="text-danger">' + result.message + '</span>');
                    toastr.error(result.message);
                }
            }
        });
    });
});