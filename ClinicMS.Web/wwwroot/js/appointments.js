$(function () {

    // ── Department → Doctor cascade ──
    $(document).on('change', '#ddlDepartment', function () {
        var deptId = $(this).val();
        var $doctor = $('#ddlDoctor');

        $doctor.html('<option value="">-- Select Doctor --</option>');

        if (!deptId) return;

        $.ajax({
            url: '/Appointments/GetDoctorsByDepartment/' + deptId,
            type: 'GET',
            success: function (doctors) {
                if (!doctors || doctors.length === 0) {
                    $doctor.append('<option value="">No doctors in this department</option>');
                    return;
                }
                doctors.forEach(function (d) {
                    $doctor.append('<option value="' + d.Value + '">' + d.Text + '</option>');
                });
            },
            error: function () {
                $doctor.append('<option value="">Error loading doctors</option>');
            }
        });
    });
    $(document).on('click', '#btnSearch', function () {
        var qs = $('#searchFrom').serialize();
        $.ajax({
            url: '/Appointments/List?' + qs,
            type: 'GET',
            success: function (html) {
                $('#pageContent').html(html);
            },
            error: function () {
                toastr.error('Search failed');
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

});