var groups;

function initialisePasswordGroups() {
    groups = {};
    groups.groupsSection = $('#PasswordGroups');
    $('section', groups.groupsSection).children('article').hide();
    $('.group-section div', groups.groupsSection).click(toggleShowHideGroupItems);
    $('.password-list input[type="text"]').click(decryptPassword);
    $('.password-list li').click(hideDecryptedPassword);
    $('.password-list button').click(deleteEntry);

    $("#modal-background, #modal-close, #modal-content input[value='Cancel']").click(function () { closeModalDialog(); });
    $(document).keydown(function(e) {
        if (e.keyCode === 27) closeModalDialog();
    });
    $('#masterpasswordentryform').submit(handleMasterPasswordEntered);
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
    var masterPasswordField = $('input[name=masterpassword]', frm);
    var rememberMasterPassword = $('input:checked[name=remembermasterpassword]', frm).val();
    var masterPassword = masterPasswordField.val();
    masterPasswordField.val('');

    console.log('submit decrypt request with form details: group=' + groupEntryId + ';entry=' + passwordEntryId + ';masterpw=' + masterPassword+';rememberMasterPassword='+rememberMasterPassword);
    if (action == 'decrypt') {
        decrypt(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword);
    } else if (action == 'delete') {
        callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword);
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
        console.log('received delete result: ' + data.deleted + ':' + data.decryptedValue + ':' + data.reason);
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