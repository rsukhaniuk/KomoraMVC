var dataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/user/inventory/getall' },
        "columns": [
            { data: 'id', "width": "5%" },
            { data: 'product.name', "width": "10%" },
            { data: 'expirationDate', "width": "5%" },
            { data: 'planDate', "width": "5%" },
            { data: 'planQuantity', "width": "5%" },
            { data: 'incomeDate', "width": "5%" },
            { data: 'incomeQuantity', "width": "5%" },
            { data: 'remaindate', "width": "5%" },
            { data: 'remainQuantity', "width": "5%" },
            { data: 'wasteDate', "width": "5%" },
            { data: 'wasteQuantity', "width": "5%" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                     <a href="/user/inventory/upsert?id=${data}" class="btn btn-primary mx-2"> <i class="bi bi-pencil-square"></i> Edit</a>
                      <a onClick=Delete('/user/inventory/delete/${data}') class="btn btn-danger mx-2"> <i class="bi bi-trash-fill"></i> Delete</a>
                    </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }
    })
}


