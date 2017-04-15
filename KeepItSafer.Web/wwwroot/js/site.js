var groups;

function initialiseCommon() {
    groups = {};
}

function initialiseCreateMasterPassword() {
    initialiseCommon();

    $('#newmasterpassword').focus();
}

function initialisePasswordGroups() {
    initialiseCommon();
    groups.groupsSection = $('#PasswordGroups');
    $('section', groups.groupsSection).children('article').hide();
    $('.group-section div', groups.groupsSection).click(toggleShowHideGroupItems);
    $('.password-list input[type="text"]').click(decryptPassword);
    $('.password-list li').click(hideDecryptedPassword);
    $('.password-list input[type="button"]').click(deleteEntry);

    $("#modal-background, #modal-close, #modal-content input[value='Cancel']").click(function () { closeModalDialog(); });
    $(document).keydown(function(e) {
        if (e.keyCode === 27) closeModalDialog();
    });
    $('#masterpasswordentryform').submit(handleMasterPasswordEntered);

    $('#addnewentryvalueencrypted').change(function() {
        $('#addnewentryvalue').attr('type', $(this).is(':checked') ? 'password' : 'text');
    });
    $('#addnewentryform').submit(handleAddNewEntry);
    $('#addnewentryform input[type="text"]').keyup(setAddNewEntryEnabledState);
    $('#addnewentryform input[type="text"]').change(setAddNewEntryEnabledState);
    setAddNewEntryEnabledState();

    $('#passwordgeneratorform').submit(handleGeneratePassword);
}

function closeModalDialog() {
    console.log('hiding any current modal dialog');
    $("#modal-content, #modal-background").removeClass("active");
}

function showModalDialog(headerMessage, level) {
    closeModalDialog();
    var header = $('#modal-content .modal-content-header');
    if ((headerMessage || '') == '') {
        header.hide();
    } else {
        header.text(headerMessage);
        header.attr('data-level', level || 'info');
        header.show();
    }
    console.log('showing dialog with message '+level+'['+headerMessage+']');

    $("#modal-content, #modal-background").addClass("active");
}

function toggleShowHideGroupItems() {
    var group = $(this);
    console.log('group clicked: ' + group.parent().attr('data-id'));
    group.next().toggle();
    $(window).scrollTop(group.position().top);
}
function hideDecryptedPassword() {
    var passwordTextbox = $('input[type="text"]', $(this));
    if (passwordTextbox.attr('data-type') == 'encrypted' &&
        passwordTextbox.attr('data-decrypted') == 'true') {
        console.log('password decrypted, reverting back to ****.');
        passwordTextbox.val('****');
        passwordTextbox.attr('data-decrypted', 'false');
        return;
    }
}
function handleMasterPasswordEntered(event) {
    var frm = $('#masterpasswordentryform');
    var action = $('input[name=action]', frm).val();
    var groupEntryId = $('input[name=group]', frm).val();
    var passwordEntryId = $('input[name=entry]', frm).val();
    var value = $('input[name=value]', frm).val();
    var valueEncrypted = $('input[name=valueEncrypted]', frm).val();
    var masterPasswordField = $('input[name=masterpassword]', frm);
    var rememberMasterPassword = $('input:checked[name=remembermasterpassword]', frm).val();
    var masterPassword = masterPasswordField.val();
    masterPasswordField.val('');

    console.log('re-submit request with form details: group=' + groupEntryId + ';entry=' + passwordEntryId + ';value=' + value + ';valueEncrypted=' + valueEncrypted + ';masterpw=' + masterPassword+';rememberMasterPassword='+rememberMasterPassword);
    if (action == 'decrypt') {
        decrypt(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword);
    } else if (action == 'delete') {
        callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword);
    } else if (action == 'add') {
        addNewEntry(groupEntryId, passwordEntryId, valueEncrypted, value, masterPassword, rememberMasterPassword);
    }
    closeModalDialog();

    event.preventDefault();
    return false;
}
function decryptPassword() {
    var passwordTextbox = $(this);
    if (passwordTextbox.attr('data-type') != 'encrypted' || passwordTextbox.attr('data-decrypted') == 'true') {
        console.log('password not encrypted or already decrypted, nothing to do.');
        passwordTextbox.select();
        return false;
    }

    // otherwise decrypt
    var passwordEntry = passwordTextbox.parents('li');
    var passwordEntryId = passwordEntry.attr('data-id');
    var groupEntryId = passwordEntry.parents('section').attr('data-id');
    console.log('password clicked: ' + passwordEntryId + ' in group ' + groupEntryId);

    decrypt(groupEntryId, passwordEntryId);
}
function decrypt(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword) {
    $.post('/api/decrypt', {
        group: groupEntryId,
        entry: passwordEntryId,
        masterPassword: masterPassword || '',
        rememberMasterPassword: rememberMasterPassword || 'false'
    })
     .done(function(data) {
        console.log('received decrypt: ' + data.decrypted + ':' + data.decryptedValue + ':' + data.reason);
        if (data.decrypted) {
            var passwordTextbox = $('section[data-id='+groupEntryId+'] li[data-id='+passwordEntryId+'] :text');
            passwordTextbox.val(data.decryptedValue);
            passwordTextbox.attr('data-decrypted', 'true');
            passwordTextbox.select();
            return;
        }

        var frm = $('#masterpasswordentryform');
        $('input[name=action]', frm).val('decrypt');
        $('input[name=group]', frm).val(groupEntryId);
        $('input[name=entry]', frm).val(passwordEntryId);
        $(':password', frm).val('');
        var headerMessage = '';
        var level = 'info';
        if (data.reason == 0) { // need to enter master password
            headerMessage = 'Please enter your master password.'
        } else if (data.reason == 1) { // incorrect master password
            headerMessage = 'Incorrect master password, please try again.'
            level = 'warn'
        } else { // some unknown error
            headerMessage = 'Unknown error decrypting. Try entering your master password again.'
            level = 'error'
        }
        showModalDialog(headerMessage, level);
        $(':password', frm).focus();
     })
     .fail(function() {
         console.log('decrypt api call failed');
     });
}
function deleteEntry() {
    var deleteBtn = $(this);
    var passwordEntry = deleteBtn.parents('li');
    var passwordEntryId = passwordEntry.attr('data-id');
    var groupEntryId = passwordEntry.parents('section').attr('data-id');
    console.log('delete button clicked: ' + passwordEntryId + ' in group ' + groupEntryId);
    if (!confirm('Are you sure you want to delete "'+passwordEntryId+'"?')) {
        return;
    }
    callDeleteEntry(groupEntryId, passwordEntryId);
}
function callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword) {
    $.post('/api/delete', {
        group: groupEntryId,
        entry: passwordEntryId,
        masterPassword: masterPassword || '',
        rememberMasterPassword: rememberMasterPassword || 'false'
    })
     .done(function(data) {
        console.log('received delete result: ' + data.deleted + ':' + data.reason);
        if (data.deleted) {
            location.reload();
            return;
        }

        var frm = $('#masterpasswordentryform');
        $('input[name=action]', frm).val('delete');
        $('input[name=group]', frm).val(groupEntryId);
        $('input[name=entry]', frm).val(passwordEntryId);
        $(':password', frm).val('');
        var headerMessage = '';
        var level = 'info';
        if (data.reason == 0) { // need to enter master password
            headerMessage = 'Please enter your master password.'
        } else if (data.reason == 1) { // incorrect master password
            headerMessage = 'Incorrect master password, please try again.'
            level = 'warn'
        } else { // some unknown error
            headerMessage = 'Unknown error deleting this entry. Try entering your master password again.'
            level = 'error'
        }
        showModalDialog(headerMessage, level);
        $(':password', frm).focus();
     })
     .fail(function() {
         console.log('delete api call failed');
     });
}
function allowAddNewEntry() {
    var allHaveValues = true;
    $('#addnewentryform input[type="text"], #addnewentryform input[type="password"]').each(function(i) {
        allHaveValues = allHaveValues && $(this).val() != '';
    });
    return allHaveValues;
}
function setAddNewEntryEnabledState() {
    if (allowAddNewEntry()) {
        $('#addnewentryform :submit').removeAttr("disabled");
    } else {
        $('#addnewentryform :submit').attr("disabled", "disabled");
    }
}
function handleAddNewEntry(event) {
    if (allowAddNewEntry()) {
        var frm = $('#addnewentryform');
        var group = $('input[name=addnewentrygroup]', frm).val();
        var entry = $('input[name=addnewentryname]', frm).val();
        var valueEncrypted = $('input:checked[name=addnewentryvalueencrypted]', frm).val() || 'false';
        var value = $('input[name=addnewentryvalue]', frm).val();

        console.log('submit add request with form details: group=' + group + ';entry=' + entry + ';valueEncrypted=' + valueEncrypted+';value='+value);
        addNewEntry(group, entry, valueEncrypted, value);
    }

    event.preventDefault();
    return false;
}
function addNewEntry(group, entry, valueEncrypted, value, masterPassword, rememberMasterPassword) {
    $.post('/api/add', {
        group: group,
        entry: entry,
        valueEncrypted: valueEncrypted,
        value: value,
        masterPassword: masterPassword || '',
        rememberMasterPassword: rememberMasterPassword || 'false'
    })
     .done(function(data) {
        console.log('received add result: ' + data.added + ':' + data.reason);
        if (data.added) {
            location.reload();
            return;
        }

        var frm = $('#masterpasswordentryform');
        $('input[name=action]', frm).val('add');
        $('input[name=group]', frm).val(group);
        $('input[name=entry]', frm).val(entry);
        $('input[name=value]', frm).val(value);
        $('input[name=valueEncrypted]', frm).val(valueEncrypted);
        $(':password', frm).val('');
        var headerMessage = '';
        var level = 'info';
        if (data.reason == 0) { // need to enter master password
            headerMessage = 'Please enter your master password.'
        } else if (data.reason == 1) { // incorrect master password
            headerMessage = 'Incorrect master password, please try again.'
            level = 'warn'
        } else { // some unknown error
            headerMessage = 'Unknown error adding this entry. Try entering your master password again.'
            level = 'error'
        }
        showModalDialog(headerMessage, level);
        $(':password', frm).focus();
     })
     .fail(function() {
         console.log('add api call failed');
     });
}
function handleGeneratePassword(event) {
    var frm = $('#passwordgeneratorform');

    var minLength = $('input[name=genpassminpasswordlength]', frm).val();
    var maxLength = $('input[name=genpassmaxpasswordlength]', frm).val();
    var allowSpecialChars = $('input:checked[name=genpassspecialchars]', frm).val() || 'false';
    var allowNumbers = $('input:checked[name=genpassnumbers]', frm).val() || 'false';

    console.log('generate passwords given min='+minLength+';max='+maxLength+';special='+allowSpecialChars+';numbers='+allowNumbers);

    event.preventDefault();
    return false;
}