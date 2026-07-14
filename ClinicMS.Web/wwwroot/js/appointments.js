$(function () {

    // ── Search button → reload grid via existing CustomAjax convention ──
    $('#btnSearch').on('click', function () {
        var filter = $('#searchFrom').serializeArray();
        var data = {};
        filter.forEach(f => data[f.name] = f.value);

        $.ajax({
            url: '/Appointments/List',
            type: 'POST',
            data: data,
            success: function (html) {
                $('#pageContent').html(html);
            }
        });
    });

    // ── Slot fetch: fires when EITHER Doctor or Date changes ──
    $(document).on('change', '#ddlDoctor, #txtDate', function () {
        var doctorId = $('#ddlDoctor').val();
        var date = $('#txtDate').val();
        var $slot = $('#ddlSlot');

        $slot.html('<option value="">-- Select Slot --</option>');
        $('#hdnStartTime').val('');

        if (!doctorId || !date) return;

        $.ajax({
            url: '/Appointments/GetSlots',
            type: 'GET',
            data: { doctorId: doctorId, date: date },
            success: function (slots) {
                if (!slots || slots.length === 0) {
                    $slot.append('<option value="">No slots available — doctor may not work this day</option>');
                    return;
                }
                slots.forEach(function (s) {
                    $slot.append('<option value="' + s.Value + '">' + s.Text + '</option>');
                });
            },
            error: function () {
                $slot.append('<option value="">Error loading slots</option>');
            }
        });
    });

    // ── Slot picked → push StartTime into hidden field for form submit ──
    $(document).on('change', '#ddlSlot', function () {
        var val = $(this).val();
        if (!val) {
            $('#hdnStartTime').val('');
            return;
        }
        var parts = val.split('|');
        $('#hdnStartTime').val(parts[0]);
    });

    // ── Cancel appointment: prompt reason, then AJAX ──
    $(document).on('click', '.btn-cancel-apt', function () {
        var id = $(this).data('id');
        var reason = prompt('Enter cancellation reason:');
        if (!reason || reason.trim() === '') {
            alert('Cancellation reason is required.');
            return;
        }
        $.ajax({
            url: '/Appointments/Cancel',
            type: 'POST',
            data: { id: id, cancelReason: reason },
            success: function (res) {
                if (res.success) {
                    $('#btnSearch').click();
                } else {
                    alert(res.message || 'Cancel failed.');
                }
            }
        });
    });

    // ── Client-side validation before Book form submits ──
    $(document).on('click', 'form[action="/Appointments/Save"] button[type="submit"]', function (e) {
        var errors = [];

        if (!$('#ddlPatient').val()) errors.push('Patient is required');
        if (!$('#ddlDepartment').val()) errors.push('Department is required');
        if (!$('#ddlDoctor').val()) errors.push('Doctor is required');
        if (!$('#txtDate').val()) errors.push('Appointment date is required');
        if (!$('#hdnStartTime').val()) errors.push('Please select a time slot');
        if (!$('#ChiefComplaint').val() || !$('#ChiefComplaint').val().trim()) errors.push('Chief complaint is required');

        if (errors.length > 0) {
            e.preventDefault();
            e.stopPropagation();
            toastr.error(errors.join('<br>'));
            return false;
        }
    });
    // ── Check-in ─────────────────────────────────────────
    $(document).on('click', '.btn-checkin-apt', function () {
        var id = $(this).data('id');
        $.ajax({
            url: '/Appointments/CheckIn',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    $('#btnSearch').click();
                } else {
                    toastr.error(res.message || 'Check-in failed.');
                }
            }
        });
    });

    // ── Complete ─────────────────────────────────────────
    $(document).on('click', '.btn-complete-apt', function () {
        var id = $(this).data('id');
        $.ajax({
            url: '/Appointments/Complete',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    location.reload();  // works on both grid page and dashboard page
                } else {
                    toastr.error(res.message || 'Complete failed.');
                }
            }
        });
    });

    // ── No-show ──────────────────────────────────────────
    $(document).on('click', '.btn-noshow-apt', function () {
        var id = $(this).data('id');
        if (!confirm('Mark this appointment as No-show?')) return;

        $.ajax({
            url: '/Appointments/NoShow',
            type: 'POST',
            data: { id: id },
            success: function (res) {
                if (res.success) {
                    $('#btnSearch').click();
                } else {
                    toastr.error(res.message || 'No-show update failed.');
                }
            }
        });
    });

});