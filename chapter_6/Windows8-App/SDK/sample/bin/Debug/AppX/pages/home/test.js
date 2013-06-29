// (c) Microsoft. All rights reserved

function displayText(text) {
    var output = document.getElementById("output");
    output.textContent = text;
}

function displayError(error) {
    var text = "ErrorCode = " + error.number.toString(16) + "\r\n";
    displayText(text + error.message);
}

function displayAlert(message) {
    return new Windows.UI.Popups.MessageDialog(message).showAsync();
}

function displayContent(text) {
    var content = document.getElementById("itemList");
    content.innerHTML = text;
}

// Use if displaying XML where displayContent() generates a warning about unsafe content
function displayContentSafe(text) {
    var content = document.getElementById("itemList");
    content.textContent = text;
}
//---------------
//
// Simple tabular display
//
//---------------
function displayList(items, fullItem) {
    var div = document.getElementById("itemList");

    var table = renderItemList(items, fullItem);
    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }
    div.appendChild(table);
}

function clearList() {
    var div = document.getElementById("itemList");
    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }
}

function renderItemList(itemList, fullItem) {
    var table = document.createElement("table");

    for (i = 0, count = itemList.length; i < count; ++i) {

        var item = itemList[i];
        var xml;
        if (fullItem) {
            xml = item.item.serialize();
        }
        else {
            xml = item.serialize();
        }

        var row = table.insertRow();

        var typeCell = row.insertCell();
        typeCell.innerText = item.type.name;

        var keyCell = row.insertCell();
        keyCell.innerText = item.key.id;

        var cell = row.insertCell();
        cell.innerText = xml;
    }

    return table;
}

function validateAndDisplayList(itemList, fullItem) {

    if (itemList == null) {
        displayAlert("Null Item List");
        return;
    }
    //
    // ensureAvailable will ensure that all PendingItems are also resolved
    // You can also use ensureAvailableAsync(startAt, count)
    //
    itemList.ensureAvailableAsync().then(
        function () {
            for (i = 0, count = itemList.length; i < count; ++i) {
                itemList[i].validate();
            }
            displayList(itemList, fullItem);
        },
        function (error) {
            displayError(error);
        },
        null
    );
}

function displaySerializableObjects(itemList) {

    var div = document.getElementById("itemList");

    var table = document.createElement("table");

    for (i = 0, count = itemList.length; i < count; ++i) {

        var item = itemList[i];
        var xml = item.serialize();

        var row = table.insertRow();
        var cell = row.insertCell();
        cell.innerText = xml;
    }

    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }
    div.appendChild(table);
}

function displayVocabMatches(itemList) {

    var div = document.getElementById("itemList");

    var table = document.createElement("table");

    for (i = 0, count = itemList.length; i < count; ++i) {

        var item = itemList[i];

        var row = table.insertRow();

        var nameCell = row.insertCell();
        nameCell.innerText = item.displayText;

        var xmlCell = row.insertCell();
        xmlCell.innerText = item.serialize();
    }

    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }
    div.appendChild(table);
}

function displayStrings(strings) {

    var div = document.getElementById("itemList");
    var table = document.createElement("table");

    if (strings != null) {
        for (i = 0, count = strings.length; i < count; ++i) {

            var item = strings[i];

            var row = table.insertRow();
            var cell = row.insertCell();
            cell.innerText = item;
        }
    }

    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }
    div.appendChild(table);
}

function ensureChildTable(div) {

    var table = div.firstChild;

    if (table != null) {
        if (table.tagName == "TABLE") {
            return table;
        }
        div.removeChild(table);
    }

    table = document.createElement("table");
    div.appendChild(table);

    return table;
}

function beginLog() {
    var div = document.getElementById("itemList");
    if (div.firstChild != null) {
        div.removeChild(div.firstChild);
    }

    return div;
}

function writeLine(parentDiv, text) {
    var table = ensureChildTable(parentDiv);
    var row = table.insertRow();
    var cell = row.insertCell();
    cell.innerText = text;
}

//----------------------------
//
// Application Startup
//
//----------------------------
var g_appSettings = createHealthVaultAppSettings();
var g_hvApp = createHealthVaultApp();
var g_serviceMethodProvider = new HealthVault.Sample.ServiceMethodProvider(g_appSettings);
startApp();

function createHealthVaultAppSettings() {
    // Below, we pass in "true" to indicate that this app supports multiple HealthVault
    // web-service instances, or is "multi-instance aware." We also pass in the URLs for
    // the default instance we'll talk to until the user signs in and authorizes the app. 
    // Because the app is multi-instance aware, the URLS in the app settings are automatically
    // updated after the user authorizes the app to point at the instance where that
    // user's account is based. The new URLs are also saved to disk and automatically
    // reloaded the next time the app starts up.
    // For more information, see the Global HealthVault Architecture article at
    // http://go.microsoft.com/?linkid=9830913.
    return new HealthVault.Foundation.HealthVaultAppSettings(
        "c24d9800-236f-48e3-b06b-bdf90a6cb0be",
        "https://platform.healthvault-ppe.com/platform/wildcat.ashx",
        "https://account.healthvault-ppe.com",
        true);
}

function createHealthVaultApp() {
    var app = new HealthVault.Foundation.HealthVaultApp(g_appSettings);
    app.appInfo.instanceName = "HV-Win8 JS";
    app.debugMode = true;

    return app;
}

function getLocalVault() {
    return g_hvApp.localVault;
}

function getCurrentRecord() {
    return g_hvApp.userInfo.authorizedRecords[0];
}

function getCurrentRecordStore() {
    return g_hvApp.localVault.recordStores.getStoreForRecord(getCurrentRecord());
}

function startApp() {
    g_hvApp.startAsync().then(
        function () {
            displayUser();
        },
        displayError,
        null);
}

function restartApp() {
    g_hvApp.resetAsync();
    startApp();
}

function authMore() {
    g_hvApp.authorizeAdditionalRecordsAsync().then(
        function () {
            displayUser();
        },
        displayError,
        null);
}

function isAuthOnServer() {
    g_hvApp.isAuthorizedOnServerAsync().then(
        function(result) {
            if (result)
                displayText("Yes");
            else
                displayText("No");
        },
        null,
        null
    );
}

function removeAppRecordAuthOnServer() {
    var promises = [];

    if (g_hvApp.userInfo && g_hvApp.userInfo.authorizedRecords) {
        for (var i = 0; i < g_hvApp.userInfo.authorizedRecords.length; i++) {
            promises.push(g_hvApp.userInfo.authorizedRecords[i].removeApplicationRecordAuthorizationAsync());
        }
    }

    WinJS.Promise.join(promises).then(function() {
        return g_hvApp.resetAsync();
    }).then(function() {
        displayText("start/restart app");
    });
}

//----------------------------
//
// Serialization Tests
//
//----------------------------
function displayUser() {
    if (!g_hvApp.hasUserInfo) {
        displayText("No user");
        return;
    }

    var userInfo = g_hvApp.userInfo;
    var xml = userInfo.serialize();
    displayText(xml);
}

function testItemFilter() {
    var filter = new HealthVault.Types.ItemFilter();

    filter.effectiveDateMax = new HealthVault.Types.DateTimeValue("12/3/2010");
    filter.updatedDateMin = new HealthVault.Types.DateTimeValue("5/6/1993");

    displayText(filter.serialize());
}

function testCodableValue() {
    var code = new HealthVault.Types.CodedValue("Snomed", "12346");

    var codableValue = new HealthVault.Types.CodableValue("Lipitor", code);

    codableValue.codes.addIfDoesNotExist(code);
    code = new HealthVault.Types.CodedValue("Snomed", "abcd");
    codableValue.codes.addIfDoesNotExist(code);

    var xml = codableValue.serialize();
    displayText(xml);

    codableValue = HealthVault.Types.CodableValue.deserialize(xml);
    displayText(codableValue.serialize());

}

function testMedication() {
    var med = makeMedication();

    var xml = med.serialize();
    med = HealthVault.ItemTypes.Medication.deserialize(xml);

    xml = med.serialize();
    displayText(xml);
}

function testCondition() {
    var cond = makeCondition();

    var xml = cond.serialize();
    cond = HealthVault.ItemTypes.Condition.deserialize(xml);

    xml = cond.serialize();
    displayText(xml);
}

function testItemQuery() {

    //var strings = new HealthVault.Collections.StringCollection();
    var query = new HealthVault.Types.ItemQuery();
    query.name = "Foo";
    query.view.typeVersions.append("33");
    query.itemIDs = new Array
    (
        HealthVault.ItemTypes.Medication.typeID,
        HealthVault.ItemTypes.Weight.typeID,
        HealthVault.ItemTypes.Height.typeID
    );
    query.keys = new Array
    (
        new HealthVault.Types.ItemKey("1234", "1.1")
    );

    query.maxResults = new HealthVault.Types.NonNegativeInt(1000);
    query.maxFullItems = new HealthVault.Types.NonNegativeInt(100);
    var xml = query.serialize();
    displayText(xml);
}

//----------------------------
//
// Cache
//
//----------------------------
function testLRUCache() {

    try {
        var maxItems = 10;
        var cache = new HealthVault.Store.ObjectCache(maxItems, false);
        var maxTestItems = 15;

        var i = 0;
        for (; i < maxItems; ++i) {
            var key = i.toString();
            var value = key + "__Value";
            cache.put(key, value);
        }

        for (j = 0; j < i / 2; ++j) {
            var value = cache.get(j.toString());
        }

        for (; i < maxTestItems; ++i) {
            var key = i.toString();
            var value = key + "__Value";
            cache.put(key, value);
        }

        displayStrings(cache.getAllKeys());
    }
    catch (ex) {
        displayError(ex);
    }
}

function setLocalStoreMemCacheSize(cacheSize) {

    g_hvApp.localVault.recordStores.maxCachedItems = cacheSize;
}

//----------------------------
//
// Local Storage
//
//----------------------------

function deleteRecordStore() {
    var record = getCurrentRecord();
    try {
        var log = beginLog();
        g_hvApp.localVault.recordStores.removeStoreForRecord(record);
        writeLine(log, "Done");

    } catch (e) {
        displayError(e);
    }
}

function putItemsInLocalStore() {

    var record = getCurrentRecord();
    var store = g_hvApp.localVault.recordStores.getStoreForRecord(record);

    var maxItems = 100;
    var weight;
    var log = beginLog();
    for (i = 0; i < maxItems; ++i) {
        weight = makeWeight();
        weight.key = HealthVault.Types.ItemKey.newKey();
        writeLine(log, weight.serialize());
        store.data.local.putAsync(weight).then(
           null,
           displayError,
            null
        );
    }
}

function getItemsFromLocalStore() {
    try {
        var record = getCurrentRecord();
        var store = g_hvApp.localVault.recordStores.getStoreForRecord(record);
        var log = beginLog();
        writeLine(log, "Starting");

        store.data.local.getItemIDsAsync().then(
            function (ids) {
                for (i = 0; i < ids.length; ++i) {
                    store.data.local.getByIDAsync(ids[i]).then(
                        function (item) {
                            writeLine(log, item.serialize());
                        },
                        displayError,
                        null
                    );
                }
            }
        );
    }
    catch (ex) {
        displayError(ex);
    }
}

//----------------------------
//
// Synchronized Store
//
//----------------------------

function renderKeys(keys) {
    var lines = new Array();
    if (keys != null) {
        for (i = 0; i < keys.length; ++i) {
            lines.push(keys[i].serialize());
        }
    }

    return lines.join("\r\n");
}

function displayKeys(keys) {
    var text = "Pending Keys Downloaded In Background:\r\n";
    text = text + renderKeys(keys);
    displayText(text);
}

function displayKeysNotFound(keys) {
    var text = "FAILED TO DOWNLOAD Keys In Background:\r\n";
    text = text + renderKeys(keys);
    displayText(text);
}

function displayPendingGetResult(result) {
    displayKeys(result.keysFound);
}

function logItems(log, keys, items) {
    for (i = 0; i < items.length; ++i) {
        var item = items[i];
        if (item != null) // CAN BE NULL
        {
            writeLine(log, item.serialize());
        }
        else {
            writeLine(log, "****LOADING***** " + keys[i].serialize());
        }
    }
}

function displayItemsWithKeys(log, record, keys, wait) {

    var store = g_hvApp.localVault.recordStores.getStoreForRecord(record);
    //
    // This waits until all pending items have arrived
    //
    if (wait == true) {
        store.data.getAsync(keys).then(
            function (items) {
                logItems(log, keys, items);
            },
            displayError
        );
    }
    else {
        // 
        // This will return local items immediately, and notify us when pending items are available
        //
        store.data.getAsync(keys, function (sender, result) { displayPendingGetResult(result); }).then(
            function (items) {
                logItems(log, keys, items);
            },
            displayError
        );
    }
}

function testSynchronizedStoreFor(typeID, awaitAll) {

    if (arguments.length < 2) {
        awaitAll = false;
    }

    displayText("");
    var log = beginLog();
    var record = getCurrentRecord();
    var filter = HealthVault.Types.ItemFilter.filterForType(typeID);
    //
    // Currently Fetch all keys from HV... but read any locally cached items..
    // Tomorrow - start storing the keys also on disk
    //
    record.getKeysAsync([filter]).then(

        function (keys) {
            if (keys.size > 0) {
                displayItemsWithKeys(log, record, keys, awaitAll);
            }
            else {
                displayText("No items found");
            }
        },
        displayError

    );
}

//----------------------------
//
// Synchronized View
//
//----------------------------

function makeViewNameFor(typeID) {
    return typeID + "_Full";
}

/// <param
function subscribeViewEvents(view) {
    view.addEventListener("error", displayError);
    view.addEventListener("itemsavailable", displayKeys);
    view.addEventListener("itemsnotfound", displayKeysNotFound);
}

function saveSyncView(view, store) {

    store.putViewAsync(view).then(
        function () {
            displayText("Saved View");
            displayText(view.data.serialize());
        },
        displayError
    );
}

function deleteSyncViewFor(typeID) {
    getCurrentRecordStore().deleteViewAsync(makeViewNameFor(typeID)).then(
        function () {
            displayText("Sync view deleted");
        });
}

function ensureSyncViewFor(typeID, store) {

    var viewName = makeViewNameFor(typeID);

    store.getViewAsync(viewName).then(
        function (view) {
            if (view == null) {

                // NO saved view. Create a new one
                view = store.createView(viewName, HealthVault.Types.ItemQuery.queryForTypeID(typeID));
                synchronizeSyncView(view, store);
            }
        }
    );
}

function synchronizeSyncView(view, store) {

    displayText("Synchronizing View");
    view.synchronizeAsync()
        .then(function () {
            displayText("Synchronized. Saving.");
            return store.putViewAsync(view)
        })
        .then(function () {  // Render saved view
            renderSynchronizedView(view);
            //renderSynchronizedViewSequential(view);
            //renderSynchronizedViewBlocking(beginLog(), view);
        }, function (error) {
            displayError(error);
        });
}

function renderSynchronizedViewBlocking(log, view) {

    for (i = 0; i < view.keyCount; ++i) {
        var item = view.ensureItemAvailableAndGetSync(i);  // BLOCKING CALL
        //var item = view.getItemSync(i); // BLOCKING CALL
        if (item == null) {
            writeLine(log, "NOT FOUND");
        }
        else {
            writeLine(log, item.serialize());
        }
    }
}

function renderSynchronizedViewChunky(log, view) {

    var chunkSize = view.keyCount;
    view.ensureItemsAvailableAndGetAsync(0, chunkSize).then(
        function (items) {
            var log = beginLog();
            for (i = 0; i < items.length; ++i) {
                var item = items[i];
                if (item == null) {
                    writeLine(log, "NOT FOUND");
                }
                else {
                    writeLine(log, item.serialize());
                }
            }
        },
        displayError
    );
}

function ensureGetItemCompleted(log, view, item, keyIndex) {

    if (item == null) {
        writeLine(log, "LOADING...");
    }
    else {
        writeLine(log, item.serialize());
    }

    if (keyIndex < view.keyCount - 1) {
        return renderSynchronizedViewItemSequential(log, view, keyIndex + 1);
    }
}

//
// Sequential rendering - but ASYNCHRONOUSLY
//
function renderSynchronizedViewItemSequential(log, view, keyIndex) {

    if (arguments.length < 3) {
        keyIndex = 0;
    }

    if (keyIndex >= view.keyCount) {
        return;
    }
    //
    // Run IN ORDER... but NOT in parallel
    //
    return view.ensureItemAvailableAndGetAsync(keyIndex)
        .then(function (item) {
            ensureGetItemCompleted(log, view, item, keyIndex);
        }, displayError
        );
}

function renderSynchronizedViewSequential(view) {

    var log = beginLog();
    renderSynchronizedViewItemSequential(log, view, 0);
}

function renderSynchronizedView(view) {

    subscribeViewEvents(view);
    
    var keyCount = view.keyCount;
    var promises = new Array();
    //
    // Collect all pending promises
    //
    for (i = 0; i < keyCount; ++i) {
        promises[i] = view.getItemAsync(i);
    }
    //
    // Now run them in PARALLEL...but return results in order
    //
    WinJS.Promise.thenEach(promises,
        function (item) {
            if (item == null) {
                return "LOADING";
            }
            else {
                return item.serialize();
            }
        }
    )
    .done(
        function (results) {
            displayStrings(results);
        }
    );
}

function synchronizeViewFor(typeID) {

    var viewName = makeViewNameFor(typeID);
    var store = getCurrentRecordStore();

    store.getViewAsync(viewName)
        .then(function (view) {
            if (view != null) {
                synchronizeSyncView(view, store);
            }
            else {
                displayText("No view to sync");
            }
        });
}

function typeIDForSyncViewTest() {
    return HealthVault.ItemTypes.BloodPressure.typeID;
}

function testSynchronizedViewFor(typeID) {

    displayText("");

    var viewName = makeViewNameFor(typeID);
    var store = getCurrentRecordStore();
    var maxAgeSeconds = 60 * 60; // 1 hour

    store.getViewAsync(viewName).then(
        function (view) {
            if (view == null) {
                ensureSyncViewFor(typeID, store);
                return;
            }

            if (view.isStale(maxAgeSeconds)) {
                synchronizeSyncView(view, store);
                return;
            }

            displayText("View is FRESH");
            renderSynchronizedView(view);
        },
        displayError
    );
}

function testRenderSynchronizedViewBlockingFor(typeID) {

    displayText("");

    var viewName = makeViewNameFor(typeID);
    var store = getCurrentRecordStore();
    store.getViewAsync(viewName)
        .then(function (view) {
            if (view != null) {
                renderSynchronizedViewBlocking(beginLog(), view);
            }
            else {
                displayText("No view has been created");
            }
        });
}

function testRenderSynchronizedChunkyFor(typeID) {

    displayText("");

    var viewName = makeViewNameFor(typeID);
    var store = getCurrentRecordStore();
    store.getViewAsync(viewName)
        .then(function (view) {
            if (view != null) {
                renderSynchronizedViewChunky(beginLog(), view);
            }
            else {
                displayText("No view has been created");
            }
        });
}

//----------------------------
//
// Vocab
//
//----------------------------

function testVocab() {

    var vocabIDs = new Array(
        HealthVault.ItemTypes.Medication.vocabForDoseUnits(),
        HealthVault.ItemTypes.Medication.vocabForStrengthUnits()
    );

    g_hvApp.vocabs.getAsync(vocabIDs).then(
        function (vocabs) {
            var xml = "";
            for (i = 0; i < vocabs.length; ++i) {
                var vocab = vocabs[i];
                xml = xml + vocab.serialize();
            }
            displayText(xml);
        },
        function (error) {
            if (error.number == HealthVault.Foundation.ServerErrorNumber.vocabNotFound) {
                displayText("Vocabulary not found");
            }
            else {
                displayError(error)
            }
        },
        null
    );
}

function testVocabSearch() {
    var rxNorm = HealthVault.ItemTypes.Medication.vocabForName();
    var text = Array.randomItemFrom("Lipitor", "Cialis", "Paxal", "Ibuprofen", "Imitrex", "Wellbutrin");

    g_hvApp.vocabs.searchAsync(rxNorm, text).then(
        function (result) {
            if (result.hasItems) {
                displayVocabMatches(result.items);
            }
        },
        displayError,
        null
    );
}

function testVocabStore() {

    var vocabIDs = new Array(
        HealthVault.ItemTypes.Medication.vocabForDoseUnits(),
        HealthVault.ItemTypes.Medication.vocabForStrengthUnits()
    );

    var maxAgeSeconds = 24 * 3600;  // 1 day...in practice, should be more like 2-3 months
    //
    // If the vocab is not available, OR stale, will trigger a download in the background
    // However the UI should NOT freeze while the vocab downloads. In fact, the download could fail.
    // The UI must keep working even if the required vocab is NOT available. 
    // Therefore, you should typically call ensureVocabs opportunistically, and EARLY
    //
    g_hvApp.localVault.vocabStore.ensureVocabsAsync(vocabIDs, maxAgeSeconds);
    //
    // This may return null if the vocab has not arrived yet
    //
    g_hvApp.localVault.vocabStore.getAsync(vocabIDs[0]).then
    (
        function (vocab) {
            displayText(vocab.serialize());
        }
    );
}
//----------------------------
//
// WIRE tests
//
//----------------------------
function testGetThings() {

    var query = new HealthVault.Types.ItemQuery();
    query.name = "GetThingsMultiple";

    var filter = new HealthVault.Types.ItemFilter(new Array(
        HealthVault.ItemTypes.Condition.typeID,
        HealthVault.ItemTypes.File.typeID,
        HealthVault.ItemTypes.Medication.typeID,
        HealthVault.ItemTypes.Procedure.typeID,
        HealthVault.ItemTypes.Weight.typeID,
        HealthVault.ItemTypes.Height.typeID
    ));
    query.filters.append(filter);
    query.maxResults = new HealthVault.Types.NonNegativeInt(100);
    //query.maxFullItems = new HealthVault.Types.NonNegativeInt(1);
    testQuery(query, true);
}

function testQuery(query, fullItem) {

    if (arguments.length < 2) {
        fullItem = false;
    }

    //query.filters[0].updatedDateMin = new HealthVault.Types.DateTime("8/17/2012");

    displayText(query.serialize());

    var record = getCurrentRecord();

    record.getAsync(query).then(
        function (itemList) {
            validateAndDisplayList(itemList, fullItem);
        },
        displayError,
        null
    );
}

//
// Forces all items to be resolved via PendingKeys...
//
function testQueryWithPendingGet(query, fullItem) {

    if (arguments.length < 2) {
        fullItem = false;
    }

    query.maxFullItems = new HealthVault.Types.NonNegativeInt(0);  // Don't inline any things...
    displayText(query.serialize());

    var record = getCurrentRecord();

    record.getAsync(query).then(
        function (itemList) {
            validateAndDisplayList(itemList, fullItem);
        },
        displayError,
        null
    );
}

function testPutItem(item) {

    var record = getCurrentRecord();
    record.putAsync(item).then(
        function (key) {
            displayText(key.serialize());
        },
        displayError,
        null
    );
}

function testRemoveThings() {
    var record = getCurrentRecord();

    var query = HealthVault.ItemTypes.Weight.queryFor();
    record.getItemsAsync(query).then(
        function (result) {
            var key = result.items[0].key;
            record.removeAsync(key);
        },
        displayError,
        null
    );
}

function testOpenFile() {
    var record = getCurrentRecord();

    var query = HealthVault.ItemTypes.File.queryFor();
    record.getAsync(query).then(
        function (itemList) {
            var firstFile = itemList[0];
            firstFile.display(record);
        },
        displayError,
        null
    );
}

function downloadFile(record, file) {

    var recordStore = g_hvApp.localVault.recordStores.getStoreForRecord(record);
    var blobStore = recordStore.blobs;
    blobStore.openWriteStreamAsync(file.key.id).then(
        function (stream) {
            file.downloadAsync(record, stream).then(
                function (success) {
                    displayAlert(success);
                },
                displayError,
                null
            )
        },
        displayError,
        null
    );
}

function testSaveFile() {

    var record = getCurrentRecord();

    var query = HealthVault.ItemTypes.File.queryFor();
    record.getAsync(query).then(
        function (itemList) {
            var firstFile = itemList[0];
            downloadFile(record, firstFile);
        },
        displayError,
        null
    );
}

function testUploadFile() {

    var record = getCurrentRecord();
    var Pickers = Windows.Storage.Pickers;
    var picker = new Pickers.FileOpenPicker();
    picker.viewMode = Pickers.PickerViewMode.list;
    picker.fileTypeFilter.replaceAll(["*"]);

    picker.pickSingleFileAsync().then
    (
        function (storageFile) {
            var file = new HealthVault.ItemTypes.File();
            file.uploadFileAsync(record, storageFile).then(
                null,
                displayError,
                null
            );
        }
    )
}

function testGetPersonalImage() {

    var record = getCurrentRecord();

    record.getAsync(HealthVault.ItemTypes.PersonalImage.queryFor()).then(
        function (items) {
            /// <param name="items" type="HealthVault.ItemTypes.ItemDataTypedList" />
            if (items.size > 0) {
                refreshPersonalImage(record, items[0]);
            }
        });
}

function displayImage(image) {

    var imageUrl = URL.createObjectURL(image, { oneTimeOnly: true });

    var imageElement = document.getElementById("recordImage");
    if (!imageElement) {
        var imageDiv = document.getElementById("recordImageCont");
        imageElement = document.createElement("img");
        imageElement.id = "recordImage";
        imageElement.src = imageUrl;
        imageDiv.appendChild(imageElement);
    }
    else {
        imageElement.src = imageUrl;
    }
}

function refreshPersonalImage(record, personalImage) {
    /// <param name="record" type="HealthVault.Foundation.IRecord" />
    /// <param name="personalImage" type="HealthVault.ItemTypes.PersonalImage" />

    var imageStreamName = "personalImage";
    var stream;
    var store = getCurrentRecordStore();
    store.blobs.openWriteStreamAsync(imageStreamName)
        .then(function (writeStream) {
            /// <param name="writeStream" type="System.IO.Stream" />
            stream = writeStream;
            return personalImage.downloadAsync(record, writeStream);
        })
        .then(function (complete) {
            stream.close();
            if (complete) {
                return store.blobs.openContentStreamAsync(imageStreamName);
            }
            else {
                displayAlert("personal image failed to download");
            }
        }, function (error) {
            displayAlert(error);
            stream.close();
        })
        .then(function (imageStream) {
            displayImage(imageStream);

            displayAlert("personal image refreshed");
        });
}

function openPersonalImage(record, personalImage) {
    /// <param name="record" type="HealthVault.Foundation.IRecord" />
    /// <param name="personalImage" type="HealthVault.ItemTypes.PersonalImage" />

    var imageStreamName = "personalImage.jpg";
    var stream;
    var store = getCurrentRecordStore();
    store.blobs.openWriteStreamAsync(imageStreamName)
        .then(function (writeStream) {
            /// <param name="writeStream" type="System.IO.Stream" />
            stream = writeStream;
            return personalImage.downloadAsync(record, writeStream);
        })
        .then(function (complete) {
            stream.close();
            if (complete) {
                return store.blobs.getStorageFileAsync(imageStreamName);
            }
            else {
                displayAlert("personal image failed to download");
            }
        }, function (error) {
            displayAlert(error);
            stream.close();
        }).then(function (file) {
            if (file) {
                var imageElement = document.getElementById("recordImage");
                if (!imageElement) {
                    var imageDiv = document.getElementById("recordImageCont");
                    imageElement = document.createElement("img");
                    imageElement.id = "recordImage";
                    imageElement.src = file.path;
                    imageDiv.appendChild(imageElement);
                }
                else {
                    imageElement.src = imageUrl;
                }

                // Windows.System.Launcher.launchFileAsync(file);
            }
        });
}

function testMakePersonalImage() {

    var record = getCurrentRecord();
    var picker = new Windows.Storage.Pickers.FileOpenPicker();

    picker.viewMode = Windows.Storage.Pickers.PickerViewMode.list;
    picker.fileTypeFilter.replaceAll([".png", ".jpg", ".jpeg"]);

    picker.pickSingleFileAsync().then(
        function(storageFile) {
            if (!storageFile) {
                return;
            }

            record.getAsync(HealthVault.ItemTypes.PersonalImage.queryFor()).then(
                function(items) {
                    var personalImage;
                    if (items.size > 0) {
                        personalImage = items[0];
                    } else {
                        personalImage = new HealthVault.ItemTypes.PersonalImage();
                    }

                    personalImage.uploadFileAsync(record, storageFile).then(
                        function() {
                            displayAlert("personal image uploaded");
                        },
                        displayError,
                        null);
                }
            );
        }
    );
}

// Basic is a singleton so you cannot just add a new one if one already exists.
function testMakeBasic() {
    var record = getCurrentRecord();
    record.getAsync(HealthVault.ItemTypes.BasicV2.queryFor()).then(function(items) {
        var basic;
        if (items && items.length > 0) {
            basic = items[0];
        } else {
            basic = new HealthVault.ItemTypes.BasicV2();
        }

        basic.gender = 'f';
        basic.birthYear = new HealthVault.Types.Year(1979);
        basic.city = 'Hollywood';
        basic.country = new HealthVault.Types.CodableValue('United States');
        basic.firstDayOfWeek = new HealthVault.Types.DayOfWeek(1);
        basic.postalCode = '90210';
        basic.state = new HealthVault.Types.CodableValue('California');
        basic.languages = [new HealthVault.Types.Language(new HealthVault.Types.CodableValue('Piglatin'), true)];

        return testPutItem(basic);
    });
}

// Personal is a singleton so you cannot just add a new one if one already exists.
function testMakePersonal() {
    var record = getCurrentRecord();
    record.getAsync(HealthVault.ItemTypes.Personal.queryFor()).then(function (items) {
        var personal;
        if (items && items.length > 0) {
            personal = items[0];
        } else {
            personal = new HealthVault.ItemTypes.Personal();
        }

        personal.name = new HealthVault.Types.Name("First", "Middle", "Last");
        var birthDate = new HealthVault.Types.StructuredDateTime();
        birthDate.date = new HealthVault.Types.Date("1982", "1", "1");
        personal.birthDate = birthDate;
        personal.bloodType = new HealthVault.Types.CodableValue("B+");
        personal.ethnicity = new HealthVault.Types.CodableValue("Indian");
        personal.nationalIdentifier = "123-45-6789";
        personal.maritalStatus = new HealthVault.Types.CodableValue("Single");
        personal.employmentStatus = "Employed";
        personal.isDeceased = new HealthVault.Types.BooleanValue(false);
        var approxDateOfDeath = new HealthVault.Types.ApproxDateTime();
        approxDateOfDeath.description = "Never";
        personal.dateOfDeath = approxDateOfDeath;
        personal.religion = new HealthVault.Types.CodableValue("Buddhism");
        personal.isVeteran = new HealthVault.Types.BooleanValue(true);
        personal.educationLevel = new HealthVault.Types.CodableValue("phd");
        personal.isDisabled = new HealthVault.Types.BooleanValue(false);
        personal.organDonor = "false";

        return testPutItem(personal);
    });
}

// This shows that given raw xml we can call a put things
// The usage is given raw xml where you do not know the type or anything about it
// you can still call put things on it to put it into HV.
function testPutThingsRaw() {
    var allergy = new HealthVault.ItemTypes.Allergy("Honey");
    var xml = allergy.item.serialize();
    var record = getCurrentRecord();
    record.putRawAsync(xml).then(function(itemKeys) {
        if (itemKeys != null) {
            var itemKeysText = '';
            for(var i =0; i < itemKeys.length; i++) {
                itemKeysText += itemKeys[i].serialize();
            }
            displayText(itemKeysText);
        } else {
            displayText("Error must have happened, no keys returned");
        }
    });
}

// This shows that given some raw XML where we do not know the exact type
// We can parse it out as a record item to get other base attributes, specifically
// a type ID to get some partial information on what is being passed in.
function testParseRaw() {
    var allergy = new HealthVault.ItemTypes.Allergy("Honey");
    var xml = allergy.item.serialize();
    xml = xml.replace(/allergy/g, 'unknown-type');
    xml = xml.replace('52bf9104-2c5e-4f1f-a66d-552ebcc53df7', 'custom-id');

    var recordItem = HealthVault.Types.RecordItem.deserialize(xml);
    displayText(recordItem.type.id);
}

function testGetThingTypes() {
    var parameters = new HealthVault.Types.ThingTypeGetParams();
    HealthVault.ItemTypes.ItemTypeManager.getItemTypeDefinitions(g_hvApp, parameters).then(
        function (results) {
            var text = '';
            for (var i = 0; i < results.length; i++) {
                var result = results[i];
                text += result.typeId + ' - ' + result.name + '<br/>'
            }
            displayContent(text);
        });
}

function testQueryPermissions(thingTypeId) {
    var record = getCurrentRecord();
    var queryParams = new HealthVault.Types.QueryPermissionsRequestParams(thingTypeId);

    record.queryPermissionsAsync(queryParams).then(
        function (response) {
            var text = '';
            var offlineAccessPermissions = response.thingTypePermission.offlineAccessPermissions;
            for (var i = 0; i < offlineAccessPermissions.length; i++) {
                var result = offlineAccessPermissions[i].value;
                text += result + '<br/>'
            }
            displayContent(text);
        },
        displayError,
        null
    );
}

function testServiceMethod(methodDelegateName) {
    g_serviceMethodProvider[methodDelegateName]().then(function (result) {
        displayContentSafe(result);
    });
}

function testLoadServiceDefinition() {
    g_hvApp.loadServiceDefinitionAsync().then(function () {
        displayContent("LiveIdAuthPolicy:  " + g_hvApp.serviceDefinition.liveIdAuthPolicy);
    });
}

function testSelectInstance() {
    var location = new HealthVault.Types.Location();
    location.country = "gb";
    g_hvApp.selectInstanceAsync(location).then(function (result) {
        displayContentSafe(HealthVault.Sample.Utility.toXml(result));
    });
}