$(document).ready(function () {
    var url = window.location.search;
    if (url.includes("isAvailable")) {
        loadData("isAvailable");
    } else {
        loadData("all");
    }
});

function loadData(status) {
    var table = $("#books").DataTable({
        "ajax": {
            "url": "/books/getAll?status=" + status,
        },
        "columns": [
            {
                "data": "title",
                "render": function (data, type, book) {
                    return "<a href='/books/edit/" + book.id + "'>" + book.title + "</a>";

                },
                "width": "25%"
            },
            {
                "data": "isbn",
            },
            {
                "data": "author.name",
            },
            {
                "data": "copies.length",
            },
            {
                "data": "id",
                "render": function (data, type, book) {
                    return `<a class="btn btn-link js-add text-success" data-book-id=${book.id}><i class="bi bi-clipboard-plus-fill"></i>&nbsp;</a>`

                },
                "className": 'dt-body-center',
            },

        ]
    });

    $("#books").on("click", ".js-add", function () {
        var button = $(this);
        $.ajax({
            url: "/books/addBookCopy/" + button.attr("data-book-id"),
            type: 'POST',
            success: function (data) {
                if (data) {
                    if (data.success) {
                        table.ajax.reload();
                        toastr.success(data.message);
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            }
        })
    })
}