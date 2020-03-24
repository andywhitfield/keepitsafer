
var notifyWindow

function initialise() {
    notifyWindow = new Notify()

    $('.groups div').click(function() { showGroup($(this).attr('data-group')) })
    $('.group-entries div:first-child a').click(function() { showAllGroups() })
    $('.group-entry-password input[data-type="encrypted"]').click(function() { decryptPassword($(this)) })
    $('.group-entries div:first-child').click(function() { hideDecryptedPasswords() })
    $('input[data-copy]').click(function() { copyPasswordToClipboard($(this)) })
    $("#modal-background, #modal-close, #modal-content input[value='Cancel']").click(function () { closeModalDialog() })
    $('#masterpasswordentryform').submit(handleMasterPasswordEntered)
    $(document).keydown(function(e) { if (e.keyCode === 27) closeModalDialog() })
}
function showAllGroups() {
    console.log('showing all groups')
    hideDecryptedPasswords()
    $('section.group-entries').hide()
    $('section.groups').show()
}
function showGroup(group) {
    console.log('showing group ' + group)
    hideDecryptedPasswords()
    $('section.group-entries[data-group="' + group + '"]').show()
    $('section.groups').hide()
    window.scrollTo(0, 0)
}
function decryptPassword(entry) {
    console.log('showing password')
    if (entry.attr('data-state') === 'decrypted')
        return
    
    let entryName = entry.parents('div[data-name]').attr('data-name')
    let groupName = entry.parents('section[data-group]').attr('data-group')
    decrypt(groupName, entryName, 'show')
}
function copyPasswordToClipboard(entry) {
    let dataCopyType = entry.attr('data-copy')
    let dataCopyName = entry.parents('div[data-name]').attr('data-name')
    if (dataCopyType === 'name') {
        copyToClipboard(dataCopyName)
    } else if (dataCopyType === 'value') {
        copyToClipboard(entry.parents('.group-entry-actions').find('.group-entry-password input').val())
    } else if (dataCopyType === 'encrypted') {
        if (entry.attr('data-state') === 'decrypted') {
            copyToClipboard(entry.attr('data-decrypted'))
            return
        }

        let passwordInput = entry.parents('.group-entry-actions').find('.group-entry-password input')
        if (passwordInput.attr('data-state') === 'decrypted') {
            copyToClipboard(passwordInput.val())
            return
        }
        let groupName = entry.parents('section[data-group]').attr('data-group')
        decrypt(groupName, dataCopyName, 'copy')
    }
}
function handleMasterPasswordEntered(event) {
    var frm = $('#masterpasswordentryform')
    var onDecrypt = $('input[name=ondecrypt]', frm).val()
    var action = $('input[name=action]', frm).val()
    var groupEntryId = $('input[name=group]', frm).val()
    var passwordEntryId = $('input[name=entry]', frm).val()
    var value = $('input[name=value]', frm).val()
    var valueEncrypted = $('input[name=valueEncrypted]', frm).val()
    var masterPasswordField = $('input[name=masterpassword]', frm)
    var rememberMasterPassword = $('input:checked[name=remembermasterpassword]', frm).val()
    var masterPassword = masterPasswordField.val()
    masterPasswordField.val('')

    console.log('re-submit request with form details: group=' + groupEntryId + ';entry=' + passwordEntryId + ';value=' + value + ';valueEncrypted=' + valueEncrypted + ';masterpw=' + masterPassword + ';rememberMasterPassword=' + rememberMasterPassword)
    if (action == 'decrypt') {
        decrypt(groupEntryId, passwordEntryId, onDecrypt, masterPassword, rememberMasterPassword)
    //} else if (action == 'delete') {
    //    callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword)
    //} else if (action == 'add') {
    //    addNewEntry(groupEntryId, passwordEntryId, valueEncrypted, value, masterPassword, rememberMasterPassword)
    }
    closeModalDialog()

    event.preventDefault()
    return false
}
function decrypt(groupEntryId, passwordEntryId, onDecrypt, masterPassword, rememberMasterPassword) {
    $.post('/api/decrypt', {
        group: groupEntryId,
        entry: passwordEntryId,
        masterPassword: masterPassword || '',
        rememberMasterPassword: rememberMasterPassword || 'false'
    })
     .done(function(data) {
        console.log('received decrypt: ' + data.decrypted + ':' + ':' + data.reason + ':group=' + groupEntryId + ':entry=' + passwordEntryId)
        if (data.decrypted) {
            groupEntryId = groupEntryId.replace(new RegExp('\'', 'g'), '\\\'')
            passwordEntryId = passwordEntryId.replace(new RegExp('\'', 'g'), '\\\'')
            if (onDecrypt === 'copy') {
                var copyButton = $("section.group-entries[data-group='" + groupEntryId + "'] div[data-name='" + passwordEntryId + "'] input[data-copy='encrypted']")
                copyButton.attr('data-state', 'decrypted')
                copyButton.attr('data-decrypted', data.decryptedValue)
                copyButton.val('Copy')
                return
            }
            var passwordTextbox = $("section.group-entries[data-group='" + groupEntryId + "'] div[data-name='" + passwordEntryId + "'] input[data-type='encrypted']")
            passwordTextbox.val(data.decryptedValue)
            passwordTextbox.attr('data-state', 'decrypted')
            passwordTextbox.select()
            return
        }

        var frm = $('#masterpasswordentryform')
        $('input[name=ondecrypt]', frm).val(onDecrypt)
        $('input[name=action]', frm).val('decrypt')
        $('input[name=group]', frm).val(groupEntryId)
        $('input[name=entry]', frm).val(passwordEntryId)
        $(':password', frm).val('')
        var headerMessage = ''
        var level = 'info'
        if (data.reason == 0) { // need to enter master password
            headerMessage = 'Please enter your master password.'
        } else if (data.reason == 1) { // incorrect master password
            headerMessage = 'Incorrect master password, please try again.'
            level = 'warn'
        } else { // some unknown error
            headerMessage = 'Unknown error decrypting. Try entering your master password again.'
            level = 'error'
        }
        showModalDialog(headerMessage, level)
        $(':password', frm).focus()
     })
     .fail(function() {
         console.log('decrypt api call failed')
     })
}
function hideDecryptedPasswords() {
    console.log('hiding any decrypted password')
    let encyptedVals = $('.group-entry-password input[data-type="encrypted"]')
    encyptedVals.val('***')
    encyptedVals.attr('data-state', 'encrypted')

    let copyButtons = $('.group-entry-copy input[data-copy="encrypted"]')
    copyButtons.attr('data-state', 'encrypted')
    copyButtons.attr('data-decrypted', '')
    copyButtons.val('Decrypt')
}
function fallbackCopyToClipboard(text) {
    let textArea = document.createElement("textarea")
    textArea.value = text
    textArea.style.top = "0"
    textArea.style.left = "0"
    textArea.style.position = "fixed"
    document.body.appendChild(textArea)
    textArea.focus()
    textArea.select()
    
    try {
        document.execCommand('copy')
    } catch (err) {
        console.error('Unable to copy', err)
    }

    document.body.removeChild(textArea)
}
function copyToClipboard(text) {
    if (!navigator.clipboard) {
        fallbackCopyToClipboard(text)
    } else {
        navigator.clipboard.writeText(text)
    }
    notifyWindow.show('Copied to clipboard', true)
}

function closeModalDialog() {
    console.log('hiding any current modal dialog')
    $('#modal-content, #modal-background').removeClass('active')
}
function showModalDialog(headerMessage, level) {
    closeModalDialog()
    var header = $('#modal-content .modal-content-header')
    if ((headerMessage || '') === '') {
        header.hide()
    } else {
        header.text(headerMessage)
        header.attr('data-level', level || 'info')
        header.show()
    }
    console.log('showing dialog with message ' + level + '[' + headerMessage + ']')

    $('#modal-content, #modal-background').addClass('active')
    $('#masterpassword').focus()
}

/* notification window */
function Notify() {
    var _self = this

    // fields

    this._notifyItem = $('<div class="notify"/>')
    this._showTimeoutId = 0

    // constructor

    this._notifyItem.hide()
    $('body').append(this._notifyItem)

    // methods

    this.show = function (message, autoClose) {
        _self._notifyItem.text(message)
        if (_self._showTimeoutId === 0) {
            console.log('showing notification')
            _self._notifyItem.show()
        } else {
            // cancel timeout
            window.clearTimeout(_self._showTimeoutId)
            _self._showTimeoutId = 0
            console.log('cleared previous timeout ' + _self._showTimeoutId)
        }

        if (autoClose) {
            _self._showTimeoutId = window.setTimeout(function () { _self._close() }, 3000)
        }
    }
    this.close = function () {
        _self._showTimeoutId = window.setTimeout(function () { _self._close() }, 750)
    }
    this._close = function () {
        console.log('closing notification')
        _self._notifyItem.hide()
        _self._notifyItem.text('')
        _self._showTimeoutId = 0
    }
}
