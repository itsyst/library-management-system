$(document).ready(function () {
    $("#formControlSelect").on("change", function () {
        var id = $(this).find(':selected')[0].attributes["data-copy-id"].value;
        Swal.fire({
            title: `Are you sure you want to delete the book copy № <p><span style='color:#d33;'>${id}</span>?<p>`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#5ea2bc',
            confirmButtonText: 'Confirm'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/books/deleteSingle/" + id,
                    type: 'DELETE',
                    success: function (data) {
                        var copy = $('#copy-item');
                        if (data) {
                            if (data.success) {
                                window.location.reload();
                                toastr.success(data.message);
                                copy.empty();
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
    });

    $("#customSwitch").on("change.bootstrapSwitch", function (e) {
        var toggle = $('input[name=toggle]').is(':checked')
        var copies = $(this).find(':checked')["prevObject"][0].dataset["copies"];
        if (toggle === true && copies != 0) {
            Swal.fire({
                title: `Are you sure you want to delete all book copies?<p>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#5ea2bc',
                confirmButtonText: 'Confirm'
            }).then((result) => {
                if (result.isConfirmed) {
                    var id = $(this).find(':checked')["prevObject"][0].dataset["bookId"];
                    $.ajax({
                        url: "/books/deleteAll/" + id,
                        type: 'DELETE',
                        success: function (data) {
                            var copy = $('#copy-item');
                            if (data) {
                                if (data.success) {
                                    window.location.reload();
                                    toastr.success(data.message);
                                    copy.empty();
                                    toggle = false;
                                }

                            }
                        }
                    })
                }
                if (result.isDismissed) {
                    window.location.reload();
                    console.log("cancel");
                    taggle = false;
                    return;
                }
            }).catch(() => {
                toastr.error("An unexpected error occurred please try again later !");
            })
        }
        else {
            window.location.reload();
            toggle = false;
            return;
        }
    });

    $("#js-destroy").on("click", ".js-delete", function () {
        var button = $(this);
        Swal.fire({
            title: `You are about to delete this book record from the database and all copies related to it. Are you sure you want to process this action? 🤔`,
            text: `You won't be able to revert this!`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#5ea2bc',
            confirmButtonText: 'Confirm'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/books/delete/" + button.attr("data-book-details"),
                    type: 'DELETE',
                    success: function (data) {
                        if (data) {
                            if (data.success) {
                                window.location.replace('/Books/index/');
                                toastr.success(data.message);

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