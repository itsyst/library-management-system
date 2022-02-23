$(document).ready(function () {
    var table = $("#authors").DataTable({
        "ajax": {
            "url": "/authors/getAll",
        },
        "columns": [
            {
                "data": "name"
            },
            {
                "data": "books.length",
            },
            {
                "data": "id",
                "render": function (data) {
                    return `<a class='btn-link' href='/authors/edit/${data}'><i class='bi bi-pencil-square' style='color:#75caeb; cursor:pointer;'></i></a>
                                <a class='btn-link js-delete' data-author-id=${data}><i class='bi bi-trash-fill' style='color: #ff4136; cursor:pointer;'></i></a>`

                },
                "className": 'text-light dt-body-center'
            }
        ]
    });

    $("#authors").on("click", ".js-delete", function () {
        var button = $(this);
        Swal.fire({
            title: `Are you sure you want to delete this author ?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#5ea2bc',
            confirmButtonText: 'Confirm'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/authors/delete/" + button.attr("data-author-id"),
                    type: 'DELETE',
                    success: function (data) {
                        if (data) {
                            if (data.success) {
                                table.ajax.reload();
                                toastr.success(data.message);
                                table.row(button.parents("tr")).remove().draw();
                            }
                            else {
                                toastr.error(data.message);
                            }

                        }
                    }
                })
            }
        }).catch(() => {
            toastr.error("An unexpected error occurred please try again later !");
        })
    })
});