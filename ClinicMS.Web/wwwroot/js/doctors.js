$(document).ready(function () {
    // Load department filter dropdown
    loadDepartmentFilter();

    // Initial grid load
    loadGrid();

    // Search form submit
    $('#searchFrom').on('submit', function (e) {
        e.preventDefault();
        loadGrid();
    });

    // Auto-filter on dropdown change
    $('#searchFrom select').on('change', function () {
        loadGrid();
    });
});

function loadDepartmentFilter() {
    $.getJSON('/Departments/ActiveList', function (data) {
        var select = $('#deptFilter');
        if (data) {
            $.each(data, function (i, dept) {
                select.append(
                    '<option value="' + dept.id + '">'
                    + dept.name + '</option>'
                );
            });
        }
    });
}
function loadGrid() {
    var params = $('#searchFrom').serialize();
    $('#pageContent').load(
        '/Doctors/List?' + params,
        function () {
            ShowMessageBox();
        }
    );
}