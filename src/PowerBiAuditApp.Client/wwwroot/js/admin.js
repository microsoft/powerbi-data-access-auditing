function SubmitForm(form) {
    form.preventDefault();

    var data = { "query": $('form').serialize() };
    $.post("/Admin/SaveReportDisplayDetails", data, function (x) {
        console.log(x);
    });

    console.log("Form submitted");
}
