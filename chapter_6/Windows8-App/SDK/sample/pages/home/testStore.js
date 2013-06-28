/// <reference src="/pages/home/hv.js" />
/// <reference src="/pages/home/hvLib.js" />
/// <reference src="/pages/home/itemMakers.js" />
/// <reference src="/pages/home/test.js" />

var HVStore = HealthVault.Store;

function writeStoreError(log, e) {
    var error = e.number - HVStore.StoreErrorNumber.errorBase;
    writeLine(log, "StoreError = " + error);
}

// Returns a promise
function store_createTestFolder() {
    var root = Windows.Storage.ApplicationData.current.localFolder;
    
    return root.createFolderAsync("Test", Windows.Storage.CreationCollisionOption.replaceExisting);
}

function renderSynchronizedType(st) {

    st.ensureItemsAvailableAndGetAsync(0, st.keyCount).then(function (items) {
        displayText("Item Count = " + items.length);
        displayList(items, true);
    });
}

function store_testChangeTable() {
    
    var log = beginLog();
    var changeTable;
    var key = HealthVault.Types.ItemKey.newKey();
    store_createTestFolder().then(function (folder) {
        writeLine(log, "track meds");
        changeTable = new HVStore.RecordItemChangeTable(folder, 10);
        return changeTable.trackChangeAsync(HealthVault.ItemTypes.Medication.typeID, key, HVStore.RecordItemChangeType.put);
    }).then(function () {
        writeLine(log, "track weight");
        return changeTable.trackChangeAsync(HealthVault.ItemTypes.Weight.typeID, HealthVault.Types.ItemKey.newKey(), HVStore.RecordItemChangeType.put);
    }).then(function () {
        writeLine(log, "removeChange");
        return changeTable.trackChangeAsync(HealthVault.ItemTypes.Medication.typeID, key, HVStore.RecordItemChangeType.remove);
    }).then(function () {
        writeLine(log, "Change Table");
        return changeTable.getChangesAsync();
    }).then(function (changes) {
        for (var i = 0; i < changes.length; ++i) {
            writeLine(log, changes[i].serialize());
        }
        writeLine(log, "GetQueue");
        return changeTable.getChangeQueueAsync();
    }).then(function (ids) {
        writeLine(log, "Queue");
        for (var i = 0; i < ids.length; ++i) {
            writeLine(log, ids[i]);
        }
        writeLine(log, "Done");
    });
}

function store_acquireLock(log, lockTable, key) {
    var lockID = 0;
    if ((lockID = lockTable.acquireLock(key)) != 0) {
        writeLine(log, "Lock Taken " + key);
        writeLine(log, lockTable.getLockInfo(key));
    }
    else {
        writeLine(log, "Already locked " + key);
    }

    return lockID;
}

function store_releaseLock(log, lockTable, key, lockID) {
    try {
        lockTable.releaseLock(key, lockID);
        writeLine(log, "Released " + key);
    }
    catch (e) {
        writeLine(log, "Release failed " + key);
        writeStoreError(log, e);
    }
}

function store_testLockTable() {

    var log = beginLog();

    var lockTable = new HVStore.RecordItemLockTable();

    var lockID1 = store_acquireLock(log, lockTable, "Item1");
    store_acquireLock(log, lockTable, "Item1");  // Should not acquire lock
    store_releaseLock(log, lockTable, "Item1", -1);  // Should fail - owner mismatch
    
    var lockID2 = store_acquireLock(log, lockTable, "Item2");
    store_releaseLock(log, lockTable, "Item1", lockID1);  // Success

    writeLine(log, "IsLocked = " + lockTable.isItemLocked("Item2"));
}

function store_getSyncType(typeID) {
    var store = getCurrentRecordStore();
    var syncType = null;

    return store.types.getAsync(typeID).then(
        function (st) {
            syncType = st;
            return st.synchronizeIfStaleAsync(3600);
            //return st.synchronizeAsync();
        },
        function (error) {
            displayError(error);
        }
    ).then(function (success) {
        return syncType;
    });
}

function store_testSyncType(typeID) {
    store_getSyncType(typeID).then(function (syncType) {
        renderSynchronizedType(syncType);
    });
}

function store_getBPType() {
    return store_getSyncType(HealthVault.ItemTypes.BloodPressure.typeID);
}

function store_getType(typeID) {
    return store_getSyncType(typeID);
}

function store_testRender() {
    store_testInit();
    store_getBPType().then(function (st) {
        renderSynchronizedType(st);
    });
}

function store_testRenderType(typeID) {
    store_testInit();
    store_getType(typeID).then(function (st) {
        renderSynchronizedType(st);
    });
}

function store_testNew() {
    store_testInit();

    var bp = makeBloodPressureRandom();
    bp.systolicValue = 9999;
    var syncType;
    store_getBPType().then(function (st) {
        syncType = st;
        return st.addNewAsync(bp);
    }).done(
        function () {
            renderSynchronizedType(syncType);
        },
        function (error) { displayError(error); }
    );
}

function store_testNewWeightBackground() {
    store_testInit();

    var store = getCurrentRecordStore();
    var weight = makeWeight();
    var syncType;

    store.types.getAsync(HealthVault.ItemTypes.Weight.typeID).then(function (st) {
        syncType = st;
        return st.addNewAsync(weight);
    }).done(
        function () {
            renderSynchronizedType(syncType);
        },
        function (error) { displayError(error); }
    );
}

function store_testNewWeight() {
    store_testInit();

    var weight = makeWeight();
    weight.value.inPounds = 217;

    var store = getCurrentRecordStore();
    store.data.newAsync(weight).done(
        function () {
            displayText("done");
        },
        function (error) {
            displayError(error);
        }
    );
}

function store_testRemove() {
    store_testInit();
    var syncType;
    store_getBPType().then(function (st) {
        syncType = st;
        return syncType.removeAsync(syncType.keyAtIndex(0));
    }).done(
        function (success) {
            renderSynchronizedType(syncType);
            if (!success) {
                var itemKey = syncType.keyAtIndex(0);
                displayText("Item locked? = " + syncType.data.locks.isItemLocked(itemKey.id));
            }
        },
        function (error) {
            displayError(error);
        }
    );
}

function store_testUpdate(updateDate) {
    store_testInit();

    var syncType;
    var itemForEdit = null;

    store_getBPType().then(function (st) {
        syncType = st;
        var itemIndex = 0;
        if (st.keyCount > 1 && updateDate) {
            itemIndex = 1;
        }
        return syncType.openForEditAsync(syncType.keyAtIndex(itemIndex));
    }).then(function (itemEditOp) {
        itemForEdit = itemEditOp;
        if (itemForEdit == null) {
            displayText("Item is locked");
        }
        var item = itemForEdit.data;
        item.systolicValue = item.systolicValue + 1;
        item.diastolicValue = item.diastolicValue + 1;
        if (updateDate) {
            item.when = HealthVault.Types.StructuredDateTime.now();
        }
        displayText(item.serialize());

        return itemForEdit;
    }).then( function () {
        if (itemForEdit != null) {
            return itemForEdit.commitAsync();
        }
        return null;
    }).done(
        function () {
            itemForEdit = null;
            renderSynchronizedType(syncType);
            syncType = null;
        },
        function (error) {
            if (itemForEdit != null) {
                itemForEdit.cancel();
            }
        }
    );
}

function store_testUpdateLock(updateDate) {
    store_testInit();

    var syncType;
    var itemForEdit = null;

    store_getBPType().then(function (st) {
        syncType = st;
        var itemIndex = 0;
        if (st.keyCount > 1 && updateDate) {
            itemIndex = 1;
        }
        return syncType.openForEditAsync(syncType.keyAtIndex(itemIndex));
    }).then(function (itemEditOp) {
        itemForEdit = itemEditOp;
        if (itemForEdit != null) {
            var item = itemForEdit.data;
            item.systolicValue = item.systolicValue + 1;
            item.diastolicValue = item.diastolicValue + 1;
            if (updateDate) {
                item.when = HealthVault.Types.StructuredDateTime.now();
            }
            displayText(item.serialize());
        }
        return itemForEdit;
    }).then(function (itemForEdit) {
        // Disable updates so this doesn't get committed immediately
        getCurrentRecordStore().data.changes.isCommitEnabled = false;
        return displayAlert("Click ok to commit changes");
    }).then(function () {
        if (itemForEdit != null) {
            return itemForEdit.commitAsync();
        }
        return null;
    }).then(function () {
            itemForEdit = null;
            renderSynchronizedType(syncType);
            // Reopen the object for editing
            return syncType.openForEditAsync(syncType.keyAtIndex(0));
    }).then(function (item) {
        itemForEdit = item;
        getCurrentRecordStore().data.changes.isCommitEnabled = true;
        return displayAlert("Click ok to CANCEL edit...");
    }).done(function () {
        if (itemForEdit != null) {
            itemForEdit.cancel();
        }
    });
}

function store_testSync() {
    var store = getCurrentRecordStore();
    var syncType;
    store.types.getAsync(HealthVault.ItemTypes.BloodPressure.typeID).then(function (st) {
        syncType = st;
        return st.synchronizeAsync();
    }).done(function (didSync) {
        if (didSync) {
            displayText("Synchronize complete");
            renderSynchronizedType(syncType);
        }
        else {
            displayText("Has changes");
        }
    });
}

function asyncForLoop(startAt, max, asyncBody, complete, error) {
    index = startAt;
    var run = function () {
        if (index >= max) {
            complete();
            return;
        }

        var promise = asyncBody(index++);
        promise.then(
            function () { run(); },
            function (ex) { error(ex); }
        );
    }
    run();
}

function store_deleteRecordStore() {
    
    var vault = getLocalVault();
    vault.recordStores.removeStoreForRecordIDAsync(getCurrentRecord().id).done(
        function () {
            displayText("Deleted");
        },
        function (error) {
            displayError(error);
        }
    );
}

function store_deleteAllLocalItems() {
    store_testInit();

    var syncType;
    store_getBPType().then(function (st) {
        syncType = st;
        var keyCount = st.keyCount;
        var loop = new asyncForLoop(0, keyCount,
            function (index) {
                var key = syncType.keyAtIndex(index);
                return syncType.data.local.removeItemAsync(key);
            },
            function () {
                displayText("Deleted local");
            },
            function (error) {
                displayError(error);
            }
         );
    });
}

var g_types = null;

function store_onEventTypeUpdated(typeID) {
    var store = getCurrentRecordStore();
    store.types.getAsync(HealthVault.ItemTypes.BloodPressure.typeID).then(function (st) {
        renderSynchronizedType(st);
    });
}

function store_testInit() {
    if (g_types == null) {
        var recordStores = getLocalVault().recordStores;
        recordStores.backgroundCommitScheduler.monitorNetworkChanges = true;
        // Typically, items will get committed automatically. However, in cases of failure, this will trigger a background retry
        recordStores.backgroundCommitScheduler.frequencyMilliseconds = 30000; // 30 seconds

        var store = getCurrentRecordStore();
        g_types = store.types;
        g_types.addEventListener("typeupdated", function (typeID) { 
            store_onEventTypeUpdated(typeID); });
    }
}

function store_enableBackgroundCommit(enabled) {
    var recordStores = getLocalVault().recordStores;
    recordStores.backgroundCommitScheduler.isEnabled = enabled;
}

function store_enableChangeCommit(enabled) {
    getCurrentRecordStore().data.changes.isCommitEnabled = enabled;
}

function store_enableImmediateCommit(enabled) {
    getCurrentRecordStore().types.immediateCommitEnabled = enabled;
}

function store_testEditLockFail() {
    store_testInit();

    var store = getCurrentRecordStore();
    var syncType;
    var lockID = 0;
    var itemKey = null;
    store.types.getAsync(HealthVault.ItemTypes.BloodPressure.typeID).then(function (st) {
        itemKey = st.keyAtIndex(0);
        lockID = store.data.locks.acquireLock(itemKey.id);
        return st.openForEditAsync(itemKey);
    }).done(
        function (editor) {
            if (editor == null) {
                displayText("Item is locked");
            }
            store.data.locks.releaseLock(itemKey.id, lockID);
        },
        function (error) {
            displayError(error);
            store.data.locks.releaseLock(itemKey.id, lockID);
        }
    );
}

function store_testRemoveLockFail() {
    store_testInit();

    var store = getCurrentRecordStore();
    var syncType;
    var lockID = 0;
    var itemKey = null;
    store.types.getAsync(HealthVault.ItemTypes.BloodPressure.typeID).then(function (st) {
        itemKey = st.keyAtIndex(0);
        lockID = store.data.locks.acquireLock(itemKey.id);
        return st.removeAsync(itemKey);
    }).done(
        function (success) {
            if (!success) {
                displayText("Item is locked");
            }
            store.data.locks.releaseLock(itemKey.id, lockID);
        },
        function (error) {
            displayError(error);
            store.data.locks.releaseLock(itemKey.id, lockID);
        }
    );
}

function store_testFilter(predicate) {
    store_testInit();

    clearList();
    var store = getCurrentRecordStore();
    store_getBPType().then(function (st) {
        return st.selectKeysAsync(predicate);
    }).done(
        function (matches) {
            displayText("Match count = " + matches.length);
            //displayList(matches, true);
            var strings = new Array();
            for (var i = 0; i < matches.length; ++i) {
                strings.push(matches[i].id);
            }
            displayStrings(strings);
        },
        function (error) {
            displayError(error);
        }
    );
}

function store_testFilterAbnormal() {
    store_testFilter(
        function (item) {
            return (item.systolicValue >= 10000);
        }
    );
}

var g_failuresEnabled = false;
function store_forceServerFailures(enabled) {
    var store = getCurrentRecordStore();

    if (enabled) {
        var remoteStore = store.data.remoteStore;
        if (g_failuresEnabled) {
            return;
        }
        var statusCode = HealthVault.Foundation.ServerErrorNumber;
        var errorCodes = [statusCode.failed, statusCode.requestTimedOut];
        var testStore = HealthVault.Store.TestRemoteStore.createServerErrorProducer(remoteStore);
        testStore.errorProbability = 1.0;
        store.data.remoteStore = testStore;
        g_failuresEnabled = true;
    }
    else  {
        var remoteStore = store.data.remoteStore;
        if (!g_failuresEnabled) {
            return;
        }
        store.data.remoteStore = new HealthVault.Store.RemoteItemStore(store.data.record);
        g_failuresEnabled = false;
    }
}

function store_testGetUpdatedRecordsSince() {
    var app = g_hvApp.getRecordsUpdatedOnServerSinceDate(new Date("1/1/2010")).done(
        function (records) {
            if (records == null || records.length == 0) {
                displayText("No updates");
                return;
            }

            var recordIDs = new Array();
            for (var i = 0; i < records.length; ++i) {
                recordIDs.push(records[i].id);
            }
            displayStrings(recordIDs);
        }
    );
}

function store_testKeysNoLocalItems() {
    store_testInit();

    store_getBPType().then(function (sType) {
        return sType.getKeysForItemsNeedingDownload(0, sType.keyCount);
    }).done(
        function (keys) {
            displayText("done");
            if (keys == null) {
                displayStrings(null);
                return;
            }
            var itemIDs = new Array();
            for (var i = 0; i < keys.length; ++i) {
                itemIDs.push(keys[i].id);
            }
            displayStrings(itemIDs);
            displayText(itemIDs.length + " keys");
        },
        function (error) {
            displayError(error);
        }
    );
}


function store_testMultiSync() {
    store_testInit();

    displayText("Multiple Sync Starting");
    displayStrings(null);

    var store = getCurrentRecordStore();
    var typeIDs = [ HealthVault.ItemTypes.BloodPressure.typeID, 
                    HealthVault.ItemTypes.Weight.typeID, 
                    HealthVault.ItemTypes.BloodGlucose.typeID];

    store.types.synchronizeTypesAsync(typeIDs, 60).done(
        function (syncedIDs) {
            displayText("Multiple Sync Done");
            if (syncedIDs != null) {
                displayStrings(syncedIDs);
            }
        },
        function (error) {
            displayError(error);
        }
    );
}

function store_testMultiRefresh() {
    store_testInit();

    displayText("Multiple Refresh Starting");
    displayStrings(null);

    var store = getCurrentRecordStore();
    var typeIDs = [HealthVault.ItemTypes.BloodPressure.typeID,
                   HealthVault.ItemTypes.Weight.typeID,
                    HealthVault.ItemTypes.BloodGlucose.typeID];
    
    store.types.getMultipleAsync(typeIDs).then(function (sTypes) {
        var refresher = new HealthVault.Store.SynchronizedViewItemRefresher(store);
        refresher.batchSize = 200; // 250 is the maximum that HealthVault will return
        for (var i = 0; i < sTypes.length; ++i) {
            refresher.addChunk(sTypes[i], 0, sTypes[i].keyCount);
        }
        return refresher.refreshAsync();
    }).done(
        function (downloadCount) {
            displayText("Refreshed " + downloadCount + " items");
        },
        function (error) {
            displayError(error);
        }
    );
}

function testWriteBack()
{
    //store_testChangeList();
    //store_testLockTable();
    //store_testChangeTable();
    //store_testSyncType(HealthVault.ItemTypes.BloodPressure.typeID);
    //store_testNew();
    store_testUpdate();
}