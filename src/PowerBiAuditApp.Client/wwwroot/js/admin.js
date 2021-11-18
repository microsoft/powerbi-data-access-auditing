function SubmitForm(form) {
    form.preventDefault();

    var data = { "query": $('form').serialize() };
    $.post("/Admin/SaveReportDisplayDetails", data);

    console.log("Form submitted");
}
