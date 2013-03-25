$(document).ready(function () {

    if (!Modernizr.input.placeholder) {
        var $inputs = $("input");
        var $forms = $("form");

        $forms.bind("submit", function () {
            var $formElements = $("input", this);
            $formElements.each(function (i, input) {
                var $inp = $(this);
                if ($inp.val() == $inp.attr("placeholder")) $inp.val("");
            });
        });

        $inputs.each(
 function () {
     if ($(this).val() == "" && $(this).attr("placeholder") != "") {
         $(this).val($(this).attr("placeholder"));
         $(this).focus(function () {
             if ($(this).val() == $(this).attr("placeholder")) $(this).val("");
         });
         $(this).blur(function () {
             if ($(this).val() == "") $(this).val($(this).attr("placeholder"));
         });
     }
 });
    }






});