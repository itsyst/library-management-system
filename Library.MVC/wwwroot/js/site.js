
var EventHandlers = (function () {


    function init() {

        $("#btn-hide-loandetails").hide();
        $("#loan-details-member").hide();

        $("#book-details-author").hide();
        $("#btn-hide-books").hide();

        $("#btn-show-loandetails").click(onClickShowLoanDetailsButton);
        $("#btn-hide-loandetails").click(onClickHideLoandDetailsButton);

        $("#btn-show-books").click(onClickShowBooks);
        $("#btn-hide-books").click(onClickHideBooks);

    }

    function onClickShowLoanDetailsButton() {
        $("#loan-details-member").show();
        $("#btn-show-loandetails").hide();
        $("#btn-hide-loandetails").show();
    }

    function onClickHideLoandDetailsButton() {
        $("#loan-details-member").hide();
        $("#btn-show-loandetails").show();
        $("#btn-hide-loandetails").hide();
    }

    function onClickShowBooks() {
        $("#book-details-author").show();
        $("#btn-show-books").hide();
        $("#btn-hide-books").show();
    }

    function onClickHideBooks() {
        $("#book-details-author").hide();
        $("#btn-show-books").show();
        $("#btn-hide-books").hide();
    }

    return {
        init
    }
})();

$(document).ready(function () {
    EventHandlers.init();
});
