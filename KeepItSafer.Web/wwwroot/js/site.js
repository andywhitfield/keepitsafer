var groups;

function initialisePasswordGroups() {
    groups = {};
    groups.groupsSection = $('#PasswordGroups');
    $('section', groups.groupsSection).children('article').hide();
    $('.group-section div', groups.groupsSection).click(toggleShowHideGroupItems);
    $('.password-list input[type="text"]').click(decryptPassword);
    $('.password-list li').click(hideDecryptedPassword);

    $("#modal-background, #modal-close, #modal-content input[value='Cancel']").click(function () { closeModalDialog(); });
    $(document).keydown(function(e) {
        if (e.keyCode === 27) closeModalDialog();
    });
    $('#masterpasswordentry').submit(decryptPasswordUsingMasterPassword);
}

function closeModalDialog() {
    console.log('hiding any current modal dialog');
    $("#modal-content, #modal-background").removeClass("active");
    $(".modal-content-value").hide();
}

function showModalDialog(contentToShow, callback) {
    closeModalDialog();
    console.log('showing dialog: ' + contentToShow);
    $("#modal-content, #modal-background").addClass("active");
    var content = $(contentToShow);
    content.show();
    if (callback != undefined) {
        callback(content);
    }
}

function toggleShowHideGroupItems() {
    var group = $(this);
    console.log('group clicked: ' + group.parent().attr('id'));
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
function decryptPassword() {
    var passwordTextbox = $(this);
    if (passwordTextbox.attr('data-type') != 'encrypted' || passwordTextbox.attr('data-decrypted') == 'true') {
        console.log('password not encrypted or already decrypted, nothing to do.');
        passwordTextbox.select();
        return false;
    }

    // otherwise decrypt
    var passwordEntry = passwordTextbox.parents('li');
    var passwordEntryId = passwordEntry.attr('id');
    var groupEntryId = passwordEntry.parents('section').attr('id');
    console.log('password clicked: ' + passwordEntryId + ' in group ' + groupEntryId);

    $.post('/api/decrypt', { group: groupEntryId, entry: passwordEntryId })
     .done(function(data) {
        console.log('received decrypt: ' + data.decrypted + ':' + data.decryptedValue + ':' + data.reason);
        if (data.decrypted) {
            passwordTextbox.val(data.decryptedValue);
            passwordTextbox.attr('data-decrypted', 'true');
            passwordTextbox.select();
            return;
        }
        if (data.reason == 0) { // need to enter master password
            showModalDialog('#modal-content-master-password-prompt', function(frm) {
                $('input[name=group]', frm).val(groupEntryId);
                $('input[name=entry]', frm).val(passwordEntryId);
                $(':password', frm).val('');
                $(':password', frm).focus();
            });
        }
     })
     .fail(function() {
         console.log('decrypt api call failed');
     });
}
function decryptPasswordUsingMasterPassword(event) {
    var frm = $('#modal-content-master-password-prompt');
    var groupEntryId = $('input[name=group]', frm).val();
    var passwordEntryId = $('input[name=entry]', frm).val();
    var masterPasswordField = $('input[name=masterpassword]', frm);
    var masterPassword = masterPasswordField.val();
    masterPasswordField.val('');

    console.log('submit decrypt request with form details: group=' + groupEntryId + ';entry=' + passwordEntryId + ';masterpw=' + masterPassword);
    closeModalDialog();

    event.preventDefault();
    return false;
}
