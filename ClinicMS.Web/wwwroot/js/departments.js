$(document).ready(function () {
    loadGrid();

    // Search button / Enter key
    $('#searchFrom').on('submit', function (e) {
        e.preventDefault();
        loadGrid();
    });

    $('#searchFrom select').on('change', function () {
        loadGrid();
    });
});

function loadGrid(pageNo) {
    var params = $('#searchFrom').serialize();

    if (pageNo) {
        params += '&PageNo=' + pageNo;
    }

    $('#pageContent').load(
        '/Departments/List?' + params,
        function () {
            ShowMessageBox();
        }
    );
}