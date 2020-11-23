var notifyWindow

function initialise() {
    notifyWindow = new Notify()

    $('.groups div').click(function() { showGroup($(this).attr('data-group')) })
    $('.group-entries div:first-child a').click(function() { showAllGroups() })
    $('.group-entry-password input[data-type="encrypted"]').click(function() { decryptPassword($(this)) })
    $('.group-entry-delete input').click(function() { deleteEntry($(this)) })
    $('.group-entries div:first-child').click(function() { hideDecryptedPasswords() })
    $('input[data-copy]').click(function() { copyPasswordToClipboard($(this)) })
    $("#masterpasswordentryform input[value='Cancel']").click(function() { closeMasterPasswordEntry() })
    $('#masterpasswordentryform').submit(handleMasterPasswordEntered)
    $('input[name="groupsfilter"]').hide().on('input', handleGroupFilter);
    $(document).keydown(handleDocumentKeyDown)

    $('#addnewentryvalueencrypted').change(function() {
        $('#addnewentryvalue').attr('type', $(this).is(':checked') ? 'password' : 'text')
    })
    $('#addnewentryform').submit(handleAddNewEntry)
    $('#addnewentryform input[type="text"]').keyup(setAddNewEntryEnabledState)
    $('#addnewentryform input[type="text"]').change(setAddNewEntryEnabledState)
    setAddNewEntryEnabledState()

    $('#passwordgeneratorform').submit(handleGeneratePassword)
}

function handleDocumentKeyDown(e) {
    switch (e.keyCode) {
        case 27: // escape
            closeMasterPasswordEntry()
            if ($('input[name="groupsfilter"]').is(':focus'))
                closeGroupFilter()
            break;
        case 81:
        case 113:
        case 70:
        case 102:
        case 83:
        case 115:
            // show quick find on 'q', 's', or 'f' (upper & lower case)
            if (document.activeElement != null && document.activeElement.tagName.toLowerCase() !== 'input' && $('section.groups').is(':visible') && !$('input[name="groupsfilter"]').is(':visible')) {
                closeGroupFilter()
                $('input[name="groupsfilter"]').show().focus().select()
                e.preventDefault()
            }
            break;
        case 13: // return/enter
            let visibleGroups = $('section.groups div[data-group]:visible')
            if ($('input[name="groupsfilter"]').is(':focus') && visibleGroups.length === 1) {
                // if only one group is displayed, let's open that group
                showGroup(visibleGroups.attr('data-group'))
                e.preventDefault()
            }
    }
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
    closeGroupFilter()
    window.scrollTo(0, 0)
}

function closeGroupFilter() {
    $('input[name="groupsfilter"]').val('').blur().hide()
    handleGroupFilter()
}

function handleGroupFilter() {
    let filter = $('input[name="groupsfilter"]').val()
    if (filter === '') {
        $('section.groups div[data-group]').show()
    } else {
        $('section.groups div[data-group]').hide()
        $('section.groups div[data-group*="' + filter + '" i]').show()
    }
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
    let frm = $('#masterpasswordentryform')
    let onDecrypt = $('input[name=ondecrypt]', frm).val()
    let action = $('input[name=action]', frm).val()
    let groupEntryId = $('input[name=group]', frm).val()
    let passwordEntryId = $('input[name=entry]', frm).val()
    let value = $('input[name=value]', frm).val()
    let valueEncrypted = $('input[name=valueEncrypted]', frm).val()
    let masterPasswordField = $('input[name=masterpassword]', frm)
    let rememberMasterPassword = $('input:checked[name=remembermasterpassword]', frm).val()
    let masterPassword = masterPasswordField.val()
    masterPasswordField.val('')

    console.log('re-submit request with form details: group=' + groupEntryId + ';entry=' + passwordEntryId + ';value=' + value + ';valueEncrypted=' + valueEncrypted + ';masterpw=' + masterPassword + ';rememberMasterPassword=' + rememberMasterPassword)
    if (action == 'decrypt') {
        decrypt(groupEntryId, passwordEntryId, onDecrypt, masterPassword, rememberMasterPassword)
    } else if (action == 'delete') {
        callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword)
    } else if (action == 'add') {
        addNewEntry(groupEntryId, passwordEntryId, valueEncrypted, value, masterPassword, rememberMasterPassword)
    }
    closeMasterPasswordEntry()

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
                let copyButton = $("section.group-entries[data-group='" + groupEntryId + "'] div[data-name='" + passwordEntryId + "'] input[data-copy='encrypted']")
                copyButton.attr('data-state', 'decrypted')
                copyButton.attr('data-decrypted', data.decryptedValue)
                copyButton.val('Copy')
                if (onDecrypt === 'copy') {
                    return
                }
                let passwordTextbox = $("section.group-entries[data-group='" + groupEntryId + "'] div[data-name='" + passwordEntryId + "'] input[data-type='encrypted']")
                passwordTextbox.val(data.decryptedValue)
                passwordTextbox.attr('data-state', 'decrypted')
                passwordTextbox.select()
                return
            }

            let frm = $('#masterpasswordentryform')
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
            showMasterPasswordEntry(headerMessage, level)
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

function closeMasterPasswordEntry() {
    console.log('hiding master password entry form')
    $('aside#MasterPasswordEntry').hide()
    $('main').show()
}

function showMasterPasswordEntry(headerMessage, level) {
    let header = $('form#masterpasswordentryform legend')
    if ((headerMessage || '') === '') {
        header.hide()
    } else {
        header.text(headerMessage)
        header.attr('data-level', level || 'info')
        header.show()
    }
    console.log('showing master password entry with message ' + level + '[' + headerMessage + ']')
    $('main').hide()
    $('aside#MasterPasswordEntry').show()
    $('#masterpassword').focus()
}

function deleteEntry(deleteBtn) {
    let entryName = deleteBtn.parents('div[data-name]').attr('data-name')
    let groupName = deleteBtn.parents('section[data-group]').attr('data-group')

    console.log('delete button clicked: ' + entryName + ' in group ' + groupName)
    if (!confirm('Are you sure you want to delete "' + entryName + '"?')) {
        return
    }
    callDeleteEntry(groupName, entryName)
}

function callDeleteEntry(groupEntryId, passwordEntryId, masterPassword, rememberMasterPassword) {
    $.post('/api/delete', {
            group: groupEntryId,
            entry: passwordEntryId,
            masterPassword: masterPassword || '',
            rememberMasterPassword: rememberMasterPassword || 'false'
        })
        .done(function(data) {
            console.log('received delete result: ' + data.deleted + ':' + data.reason)
            if (data.deleted) {
                location.reload(true)
                return
            }

            let frm = $('#masterpasswordentryform')
            $('input[name=action]', frm).val('delete')
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
                headerMessage = 'Unknown error deleting this entry. Try entering your master password again.'
                level = 'error'
            }
            showMasterPasswordEntry(headerMessage, level)
        })
        .fail(function() {
            console.log('delete api call failed')
        })
}

function allowAddNewEntry() {
    var allHaveValues = true
    $('#addnewentryform input[type="text"], #addnewentryform input[type="password"]').each(function(i) {
        allHaveValues = allHaveValues && $(this).val() != ''
    })
    return allHaveValues
}

function setAddNewEntryEnabledState() {
    if (allowAddNewEntry()) {
        $('#addnewentryform :submit').removeAttr("disabled")
    } else {
        $('#addnewentryform :submit').attr("disabled", "disabled")
    }
}

function handleAddNewEntry(event) {
    if (allowAddNewEntry()) {
        let frm = $('#addnewentryform')
        let group = $('input[name=addnewentrygroup]', frm).val()
        let entry = $('input[name=addnewentryname]', frm).val()
        let valueEncrypted = $('input:checked[name=addnewentryvalueencrypted]', frm).val() || 'false'
        let value = $('input[name=addnewentryvalue]', frm).val()

        console.log('submit add request with form details: group=' + group + ';entry=' + entry + ';valueEncrypted=' + valueEncrypted + ';value=' + value)
        addNewEntry(group, entry, valueEncrypted, value)
    }

    event.preventDefault()
    return false
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
            console.log('received add result: ' + data.added + ':' + data.reason)
            if (data.added) {
                location.reload(true)
                return
            }

            let frm = $('#masterpasswordentryform')
            $('input[name=action]', frm).val('add')
            $('input[name=group]', frm).val(group)
            $('input[name=entry]', frm).val(entry)
            $('input[name=value]', frm).val(value)
            $('input[name=valueEncrypted]', frm).val(valueEncrypted)
            $(':password', frm).val('')
            var headerMessage = ''
            var level = 'info'
            if (data.reason == 0) { // need to enter master password
                headerMessage = 'Please enter your master password.'
            } else if (data.reason == 1) { // incorrect master password
                headerMessage = 'Incorrect master password, please try again.'
                level = 'warn'
            } else { // some unknown error
                headerMessage = 'Unknown error adding this entry. Try entering your master password again.'
                level = 'error'
            }
            showMasterPasswordEntry(headerMessage, level)
        })
        .fail(function() {
            console.log('add api call failed')
        })
}

function handleGeneratePassword(event) {
    let frm = $('#passwordgeneratorform')

    let minLength = $('input[name=genpassminpasswordlength]', frm).val()
    let maxLength = $('input[name=genpassmaxpasswordlength]', frm).val()
    let allowSpecialChars = $('input:checked[name=genpassspecialchars]', frm).val() || 'false'
    let allowNumbers = $('input:checked[name=genpassnumbers]', frm).val() || 'false'

    console.log('generate passwords given min=' + minLength + ';max=' + maxLength + ';special=' + allowSpecialChars + ';numbers=' + allowNumbers)

    $.post('/api/genpasswords', {
            minLength: minLength,
            maxLength: maxLength,
            allowSpecialCharacters: allowSpecialChars,
            allowNumbers: allowNumbers
        })
        .done(function(data) {
            console.log('received generate passwords result: ' + data.passwords)
            let passwordDiv = $('#genpassoutput')
            passwordDiv.empty()

            if (data.passwords.length == 0) {
                passwordDiv.text('Word dictionary loading...please try again in a few moments.')
                return
            }

            $.each(data.passwords, function(k, v) {
                let newInput = $('<input />').attr('type', 'text').val(v)
                passwordDiv.append(newInput).append('<br />')
            });
            $('input', passwordDiv).mousedown(function(e) {
                e.stopPropagation()
                if ($(this).is(':focus')) { return }
                let inputEl = $(this)[0]
                window.setTimeout(function() {
                    inputEl.setSelectionRange(0, 9999)
                }, 10)
            })
        })
        .fail(function() {
            console.log('generate password api call failed')
        })

    event.preventDefault()
    return false
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

    this.show = function(message, autoClose) {
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
            _self._showTimeoutId = window.setTimeout(function() { _self._close() }, 3000)
        }
    }
    this.close = function() {
        _self._showTimeoutId = window.setTimeout(function() { _self._close() }, 750)
    }
    this._close = function() {
        console.log('closing notification')
        _self._notifyItem.hide()
        _self._notifyItem.text('')
        _self._showTimeoutId = 0
    }
}