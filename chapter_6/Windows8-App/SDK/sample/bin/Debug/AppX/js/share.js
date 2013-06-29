(function() {
    "use strict";
    
    // Variable to store the ShareOperation object
    var shareOperation = null;
    
    // Variable to store the visibility of the Extended Sharing section
    var extendedSharingCollapsed = true;

    var weightDataContract = "HealthVault.ItemTypes." + HealthVault.ItemTypes.Weight.typeID;
    var bloodPressureDataContract = "HealthVault.ItemTypes." + HealthVault.ItemTypes.BloodPressure.typeID;
    
    /// <summary>
    /// Helper function to display received sharing content
    /// </summary>
    /// <param name="type">
    /// The type of content received
    /// </param>
    /// <param name="value">
    /// The value of the content
    /// </param>
    function displayContent(label, content, preformatted) {
        var labelNode = document.createElement("strong");
        labelNode.innerText = label;

        document.getElementById("contentValue").appendChild(labelNode);

        if (preformatted) {
            var pre = document.createElement("pre");
            pre.innerHTML = content;
            document.getElementById("contentValue").appendChild(pre);
        }
        else {
            document.getElementById("contentValue").appendChild(document.createTextNode(content));
        }
        document.getElementById("contentValue").appendChild(document.createElement("br"));
    }
    
    /// <summary>
    /// Handler executed on activation of the target
    /// </summary>
    /// <param name="eventArgs">
    /// Arguments of the event. In the case of the Share contract, it has the ShareOperation
    /// </param>
    function activatedHandler(eventObject) {
        // In this sample we only do something if it was activated with the Share contract
        if (eventObject.detail.kind === Windows.ApplicationModel.Activation.ActivationKind.shareTarget) {
            eventObject.setPromise(WinJS.UI.processAll());

            // We receive the ShareOperation object as part of the eventArgs
            shareOperation = eventObject.detail.shareOperation;

            // We queue an asychronous event so that working with the ShareOperation object does not
            // block or delay the return of the activation handler.
            WinJS.Application.queueEvent({ type: "shareready" });
        }
    }
    
    /// <summary>
    /// Handler executed when ready to share; handling the share operation should be performed
    /// outside the activation handler
    /// </summary>
    function shareReady(eventArgs) {
        HealthVault.ItemTypes.ItemTypeManager.init();

        document.getElementById("title").innerText = shareOperation.data.properties.title;
        document.getElementById("description").innerText = shareOperation.data.properties.description;

        var contentString = "";
        
        // Display a thumbnail if available
        if (shareOperation.data.properties.thumbnail) {
            shareOperation.data.properties.thumbnail.openReadAsync().done(function (thumbnailStream) {
                var thumbnailBlob = MSApp.createBlobFromRandomAccessStream(thumbnailStream.contentType, thumbnailStream);
                var thumbnailUrl = URL.createObjectURL(thumbnailBlob, { oneTimeOnly: true });
                document.getElementById("thumbnailImage").src = thumbnailUrl;
                document.getElementById("thumbnailArea").className = "unhidden";
            });
        }

        if (shareOperation.data.contains(weightDataContract)) {
            shareOperation.data.getTextAsync(weightDataContract).done(function (customFormatString) {
                var items = HealthVault.Types.RecordItem.deserializeToTypedItems(customFormatString);
                contentString += "Weight: <br/>";
                for (var i = 0; i < items.size; i++) {
                    contentString = contentString + items[i].when.toString() + " " + items[i].value.inKg + "<br/>";
                }
                displayContent("Weight: ", contentString, true);
            });
        }
        
        if (shareOperation.data.contains(bloodPressureDataContract))
        {
            shareOperation.data.getTextAsync(bloodPressureDataContract).done(function (customFormatString) {
                var items = HealthVault.Types.RecordItem.deserializeToTypedItems(customFormatString);
                contentString += "Blood Pressure: <br/>";
                for (var i = 0; i < items.size; i++) {
                    contentString = contentString + items[i].when.toString() + " " + items[i].systolic.value + "/" + items[i].diastolic.value + "<br/>";
                }
                displayContent("Blood Pressure: ", contentString, true);
            });
        }
    }

    /// <summary>
    /// Sets the blob URL for an image element based on a reference to an image stream within a resource map
    /// </summary>
    function setResourceMapURL(streamReference, imageElement) {
        if (streamReference) {
            streamReference.openReadAsync().done(function (imageStream) {
                if (imageStream) {
                    var blob = MSApp.createBlobFromRandomAccessStream(imageStream.contentType, imageStream);
                    var url = URL.createObjectURL(blob, { oneTimeOnly: true });
                    imageElement.src = url;
                }
            }, function (e) {
                imageElement.alt = "Failed to load";
            });
        }
    }

    /// <summary>
    /// Use to simulate that an extended share operation has started
    /// </summary>
    function reportStarted() {
        shareOperation.reportStarted();
    }

    /// <summary>
    /// Use to simulate that an extended share operation has retrieved the data
    /// </summary>
    function reportDataRetrieved() {
        shareOperation.reportDataRetrieved();
    }

    /// <summary>
    /// Use to simulate that an extended share operation has reached the status "submittedToBackgroundManager"
    /// </summary>
    function reportSubmittedBackgroundTask() {
        shareOperation.reportSubmittedBackgroundTask();
    }

    /// <summary>
    /// Submit for extended share operations. Can either report success or failure, and in case of success, can add a quicklink.
    /// This does NOT take care of all the prerequisites (such as calling reportExtendedShareStatus(started)) before submitting.
    /// </summary>
    function reportError() {
        var errorText = document.getElementById("extendedShareErrorMessage").value;
        shareOperation.reportError(errorText);
    }

    /// <summary>
    /// Call the reportCompleted API
    /// </summary>
    function reportCompleted() {
        shareOperation.reportCompleted();
    }

    /// <summary>
    /// Expand/collapse the Extended Sharing div
    /// </summary>
    function expandoClick() {
        if (extendedSharingCollapsed) {
            document.getElementById("extendedSharing").className = "unhidden";
            // Set expando glyph to up arrow
            document.getElementById("expandoGlyph").innerHTML = "&#57360;";
            extendedSharingCollapsed = false;
        } else {
            document.getElementById("extendedSharing").className = "hidden";
            // Set expando glyph to down arrow
            document.getElementById("expandoGlyph").innerHTML = "&#57361;";
            extendedSharingCollapsed = true;
        }
    }
    

    // Initialize the activation handler
    WinJS.Application.addEventListener("activated", activatedHandler, false);
    WinJS.Application.addEventListener("shareready", shareReady, false);
    WinJS.Application.start();

    function initialize() {
        document.getElementById("reportCompleted").addEventListener("click", /*@static_cast(EventListener)*/reportCompleted, false);
        document.getElementById("expandoClick").addEventListener("click", /*@static_cast(EventListener)*/expandoClick, false);
        document.getElementById("reportStarted").addEventListener("click", /*@static_cast(EventListener)*/reportStarted, false);
        document.getElementById("reportDataRetrieved").addEventListener("click", /*@static_cast(EventListener)*/reportDataRetrieved, false);
        document.getElementById("reportSubmittedBackgroundTask").addEventListener("click", /*@static_cast(EventListener)*/reportSubmittedBackgroundTask, false);
        document.getElementById("reportError").addEventListener("click", /*@static_cast(EventListener)*/reportError, false);
    }

    document.addEventListener("DOMContentLoaded", initialize, false);
})();