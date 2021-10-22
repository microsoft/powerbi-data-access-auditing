// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

function LoadReport(embedToken, embedUrl) {

    embedUrl = decodeURIComponent(embedUrl.replace(/&amp;/g, "&"));

    const models = window["powerbi-client"].models;
    const reportContainer = $("#report-container").get(0);

    const reportLoadConfig = {
        type: "report",
        tokenType: models.TokenType.Embed,
        accessToken: embedToken,
        // You can embed different reports as per your need
        embedUrl: embedUrl,
        // Enable this setting to remove gray shoulders from embedded report
        settings: {
            visualRenderedEvents: true,
            //background: models.BackgroundType.Transparent,
            panes: {
                filters: {
                    visible: false
                },
                pageNavigation: {
                    visible: false
                }
            },
            visualSettings: {
                visualHeaders: [
                    {
                        settings: {
                            visible: false
                        }
                        // No selector - Hide visual header for all the visuals in the report
                    }
                ]
            }
         }
    };

    // Use the token expiry to regenerate Embed token for seamless end user experience
    // Refer https://aka.ms/RefreshEmbedToken
    //tokenExpiry = embedParams.EmbedToken.Expiration;

    // Embed Power BI report when Access token and Embed URL are available
    var report = powerbi.embed(reportContainer, reportLoadConfig);

    // Clear any other loaded handler events
    report.off("loaded");

    // Triggers when a report schema is successfully loaded
    report.on("loaded", function () {
        console.log("Report load successful");
    });



    // Clear any other rendered handler events
    report.off("rendered");

    // Triggers when a report is successfully embedded in UI
    report.on("rendered", function () {
        Audit(report);
        console.log("Report render successful");

    });

    report.on('visualRendered', function (event) {
        console.log("Visual Rendered");
        console.log(event);
        console.log(event.detail);
        console.log($('#' + event.detail.name).length);
    });


    report.on("dataSelected", function (event) {
        const data = event.detail;
        jQuery.ajax({
            url: "ContextMenu/Index",
            type: "POST",
            data: JSON.stringify(data),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (returnData) {
                if (returnData.url !== "") {
                    window.location.assign(returnData.url);
                }
            }
        });
    });

    // Clear any other error handler events
    report.off("error");
            
    // Handle embed errors
    report.on("error", function (event) {
        const errorMsg = event.detail;
            
        // Use errorMsg variable to log error in any destination of choice
        console.error('Error', errorMsg);
        return;
    });
}

async function Audit(report) {
    // Retrieve the page collection and get the visuals for the active page.
    try {
        const pages = await report.getPages();

        // Retrieve the page that contain the visual. For the sample report it will be the active page
        const page = pages.filter(function (page) {
            return page.isActive;
        })[0];

        const visuals = await page.getVisuals();

        visuals.forEach(async function (visual) {
            // Exports visual data
            const result = await visual.exportData(models.ExportDataType.Summarized);

            //console.log(result.data);
        });

    }
    catch (errors) {
        console.log(errors);
    }
}



