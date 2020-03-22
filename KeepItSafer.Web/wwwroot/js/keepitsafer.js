
var notifyWindow

function initialise() {
    notifyWindow = new Notify()

    $('.groups div').click(function() {
        var showGroup = $(this).attr('data-group')
        console.log('showing group ' + showGroup)
        hideDecryptedPasswords()
        $('section.group-entries[data-group="' + showGroup + '"]').show()
        $('section.groups').hide()
        window.scrollTo(0, 0)
    })
    $('.group-entries div:first-child a').click(function() {
        console.log('going back')
        hideDecryptedPasswords()
        $('section.group-entries').hide()
        $('section.groups').show()
    })
    $('.group-entry-password input[data-type="encrypted"]').click(function() {
        console.log('showing password')
        if ($(this).attr('data-state') === 'decrypted')
            return
        
        showModalDialog('some message', 'warn')

        $(this).val('decrypted password')
        $(this).attr('data-state', 'decrypted')
        console.log('password decrypted')
    })
    $('.group-entries div:first-child').click(function() {
        hideDecryptedPasswords()
    })
    $('input[data-copy]').click(function() {
        let dataCopyType = $(this).attr('data-copy')
        let dataCopyName = $(this).parents('div[data-name]').attr('data-name')
        if (dataCopyType === 'name') {
            copyToClipboard(dataCopyName)
        } else if (dataCopyType === 'value') {
            copyToClipboard($(this).parents('.group-entry-actions').find('.group-entry-password input').val())
        } else if (dataCopyType === 'encrypted') {
            let passwordInput = $(this).parents('.group-entry-actions').find('.group-entry-password input')
            if (passwordInput.attr('data-state') === 'decrypted') {
                copyToClipboard(passwordInput.val())
                return
            }
            showModalDialog('some message: need to decrypt', 'info')
        }
    })
    $("#modal-background, #modal-close, #modal-content input[value='Cancel']").click(function () {
        closeModalDialog()
    })
    $(document).keydown(function(e) {
        if (e.keyCode === 27) closeModalDialog()
    })
}

function hideDecryptedPasswords() {
    console.log('hiding any decrypted password')
    let encyptedVals = $('.group-entry-password input[data-type="encrypted"]')
    encyptedVals.val('***')
    encyptedVals.attr('data-state', 'encrypted')
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
        let successful = document.execCommand('copy')
        let msg = successful ? 'successful' : 'unsuccessful'
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
