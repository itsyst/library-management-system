$(document).ready(function () {
    function Delete(url) {
        Swal.fire({
            title: `Are you sure you want to delete this member ?`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#5ea2bc',
            confirmButtonText: 'Confirm'
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: url,
                    type: 'DELETE',
                    success: function (data) {
                        if (data) {
                            if (data.success) {
                                window.location.reload();
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
    }
});