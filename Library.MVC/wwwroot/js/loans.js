$(document).ready(function () {
    loadData();
});

function loadData() {
    $("#loans").on("click", ".js-delete", function () {
        var button = $(this);
        Swal.fire({
            title: `You are about to delete this loan record from the database. Are you sure you want to process this action? 🤔`,
            text: `You won't be able to revert this!`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#5ea2bc',
            confirmButtonText: 'Confirm'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: "/loans/delete/" + button.attr("data-loan-id"),
                    type: 'DELETE',
                    success: function (data) {
                        if (data) {
                            if (data.success) {
                                window.location.reload();
                                toastr.success(data.message, { timeOut: 10000 });
                            }
                            else {
                                toastr.error(data.message, { timeOut: 10000 });
                            }
                        }
                    }
                })
            }
        }).catch(() => {
            toastr.error("An unexpected error occurred please try again later !");
        })
    })
}