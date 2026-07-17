$(function () {
    $('#btnInvoiceSearch').on('click', function () {
        var filter = $('#searchFrom').serializeArray();
        var data = {};
        filter.forEach(f => data[f.name] = f.value);

        $.ajax({
            url: '/Invoices/List',
            type: 'GET',
            data: data,
            success: function (html) {
                $('#pageContent').html(html);
            }
        });
    });

    $('#searchFrom select[name="Status"], #searchFrom input[name="FromDate"], #searchFrom input[name="ToDate"]').on('change', function () {
        $('#btnInvoiceSearch').click();
    });
});

$(function () {
    let rowIndex = 0;

    function recalcTotal() {
        let total = 0;
        $('#invItemsBody tr').each(function () {
            const qty = parseFloat($(this).find('.inv-qty').val()) || 0;
            const price = parseFloat($(this).find('.inv-price').val()) || 0;
            const lineTotal = qty * price;
            $(this).find('.inv-line-total').text(lineTotal.toFixed(2));
            total += lineTotal;
        });
        $('#invTotalDisplay').text(total.toFixed(2));
    }

    function addInvRow(data) {
        data = data || { description: '', quantity: 1, unitPrice: 0 };
        const idx = rowIndex++;
        const row = `
            <tr data-row-index="${idx}">
                <td><input type="text" class="form-control form-control-sm inv-desc" value="${data.description}" required /></td>
                <td><input type="number" class="form-control form-control-sm inv-qty" value="${data.quantity}" min="1" /></td>
                <td><input type="number" class="form-control form-control-sm inv-price" value="${data.unitPrice}" min="0" step="0.01" /></td>
                <td class="inv-line-total text-end pt-2">0.00</td>
                <td><button type="button" class="btn btn-sm btn-danger remove-inv-row">&times;</button></td>
            </tr>`;
        $('#invItemsBody').append(row);
    }

    // Seed with server-provided initial items (e.g. consultation fee)
    if (window.__seedInvoiceItems && window.__seedInvoiceItems.length > 0) {
        window.__seedInvoiceItems.forEach(function (item) {
            addInvRow({ description: item.Description, quantity: item.Quantity, unitPrice: item.UnitPrice });
        });
    } else {
        addInvRow();
    }
    recalcTotal();

    $('#addInvRowBtn').on('click', function () {
        addInvRow();
        recalcTotal();
    });

    $(document).on('click', '.remove-inv-row', function () {
        if ($('#invItemsBody tr').length <= 1) {
            toastr.warning('At least one line item is required.');
            return;
        }
        $(this).closest('tr').remove();
        recalcTotal();
    });

    $(document).on('input', '.inv-qty, .inv-price', recalcTotal);

    $('#saveInvoiceBtn').on('click', function () {
        const items = [];
        $('#invItemsBody tr').each(function () {
            const row = $(this);
            items.push({
                Description: row.find('.inv-desc').val(),
                Quantity: parseInt(row.find('.inv-qty').val()) || 1,
                UnitPrice: parseFloat(row.find('.inv-price').val()) || 0
            });
        });

        if (items.length === 0 || !items[0].Description) {
            toastr.error('Add at least one valid line item.');
            return;
        }

        const payload = {
            AppointmentId: $('#invAppointmentId').val(),
            Items: items
        };

        $.ajax({
            url: '/Invoices/Generate',
            type: 'POST',
            contentType: 'application/json',
            headers: {
                'RequestVerificationToken': $('#invoiceForm input[name="__RequestVerificationToken"]').val()
            },
            data: JSON.stringify(payload),
            success: function (result) {
                if (result.success) {
                    toastr.success('Invoice generated.');
                    setTimeout(function () { window.location.href = result.url; }, 800);
                } else {
                    $('#invStatus').html('<span class="text-danger">' + result.message + '</span>');
                    toastr.error(result.message);
                }
            }
        });
    });
});
$(document).on('click', '#btnPay', function () {

    var id = $(this).data('id');
    var method = $('#ddlPaymentMethod').val();

    $.ajax({
        url: '/Invoices/Pay',
        type: 'POST',
        data: {
            id: id,
            PaymentMethod: method,
            __RequestVerificationToken: $('#payForm input[name="__RequestVerificationToken"]').val()
        },
        success: function (res) {
            if (res.success) {
                toastr.success('Invoice marked as paid.');
                setTimeout(function () {
                    location.reload();
                }, 800);
            }
            else {
                $('#payStatus').html('<span class="text-danger">' + res.message + '</span>');
                toastr.error(res.message);
            }
        }
    });

});