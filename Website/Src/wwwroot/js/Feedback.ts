﻿// On hiding the modal, reset it.
$("#feedbackModal").on("hidden.bs.modal", () =>
{
    $("#feedbackComments").val("");
    $("#feedbackSubmit").removeAttr("disabled");

    // Clear any errors
    const container = $("span[data-valmsg-for='feedbackComments']");
    container.addClass("field-validation-valid").removeClass("field-validation-error");
    container.text("");
});

$("#feedbackForm").submit((event: JQuery.Event<HTMLFormElement>) =>
{
    function handleSuccess(): void
    {
        $("#feedbackModal").modal("hide");
    }

    function handleError(): void
    {
        // Custom error message
        const container = $("span[data-valmsg-for='feedbackComments']");
        container.removeClass("field-validation-valid").addClass("field-validation-error");
        container.text("Something went wrong. Please try again later.");

        $("#feedbackSubmit").removeAttr("disabled");
    }

    const form = event.target;
    if ($(form).valid())
    {
        $("#feedbackSubmit").attr("disabled", "disabled");
        $.ajax({
            data: $(form).serialize(),
            error: handleError,
            success: handleSuccess,
            type: form.method,
            url: form.action,
        });
    }

    return false;
});
