var original = $('#reportDisplayForm').serialize();

$('.security-group-tags').select2({
    ajax: {
        url: "/Admin/GetSecurityGroups",
        datatype: 'json',
        delay: 250,
        data: function (params) {
            return {
                term: params.term
            };
        },
        processResults: function (data) {
            var results = $.map(data, function (obj) {
                return {"id": `{"id": "${obj["id"]}", "name": "${obj["name"]}"}`, "text": obj["name"]};
            });

            return {
                results: results
            };
        },
    },
    placeholder: 'Search for a security group',
    minimumInputLength: 4
});

// Show/hide text input on toggle change of state
$('.linked-input').on('change', function () {
    $(this).next().toggle();
});

$('#reportDisplayForm').on('change', function () {
    if ($('#reportDisplayForm').serialize() != original) {
        $('button[type="submit"]').attr("disabled", false);
    } else {
        $('button[type="submit"]').attr("disabled", true);
    };
});

$(window).on('beforeunload', function () {
    if ($('#reportDisplayForm').serialize() != original) {
        return "Are you want to leave before saving your changes?";
    }
});


$('#report-saved-alert .btn-close').on('click', () => $('#report-saved-alert').addClass("d-none"));
$('#report-error-alert .btn-close').on('click', function () {
    $('#report-error-alert').addClass('d-none');
    $('#report-error-alert').empty();
});

$('#reportDisplayForm').submit(function (event) {
    event.preventDefault();

    $('.linked-input:not(:checked)').next().val("");

    var data = { "query": $('#reportDisplayForm').serialize() };
    $.post("/Admin/SaveReportDisplayDetails", data)
        .done(() => {
            $('#report-saved-alert').removeClass('d-none');
            $('.view-main').scrollTop(0);

            $('button[type="submit"]').attr("disabled", true);
            original = $('#reportDisplayForm').serialize();
        })
        .fail((response) => {
            $('#report-error-alert').removeClass('d-none');
            $('.view-main').scrollTop(0);
        });
});
