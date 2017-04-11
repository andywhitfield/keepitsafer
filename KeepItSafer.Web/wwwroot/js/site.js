var groups;

function initialisePasswordGroups() {
    groups = {};
    groups.groupsSection = $('#PasswordGroups');
    $('section', groups.groupsSection).children('article').hide();
    $('.group-section div', groups.groupsSection).click(toggleShowHideGroupItems);
    $('.password-list input[type="text"]').click(decryptPassword);
    $('.password-list li').click(hideDecryptedPassword);
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
    if (passwordTextbox.attr('data-type') != 'encrypted') {
        console.log('password not encrypted, nothing to do.');
        return false;
    }

    // if decrypted, revert back to the masked value
    if (passwordTextbox.attr('data-decrypted') == 'true') {
        console.log('password already decrypted, nothing to do.');
        return false;
    }

    // otherwise decrypt
    var passwordEntry = passwordTextbox.parents('li');
    var passwordEntryId = passwordEntry.attr('id');
    var groupEntryId = passwordEntry.parents('section').attr('id');
    console.log('password clicked: ' + passwordEntryId + ' in group ' + groupEntryId);

    $.post('/api/decrypt', { group: groupEntryId, entry: passwordEntryId })
     .done(function(data) {
        console.log('received decrypt: ' + data.decrypted + ':' + data.decryptedValue);
        passwordTextbox.val(data.decryptedValue);
        passwordTextbox.attr('data-decrypted', 'true');
     })
     .fail(function() {
         console.log('decrypt failed');
     });
}
