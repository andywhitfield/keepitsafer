var groups = {};

function initialisePasswordGroups() {
    groups.groupsSection = $('#PasswordGroups');
    $('section', groups.groupsSection).children('article').hide();
    $('.group-section div', groups.groupsSection).click(toggleShowHideGroupItems);
}

function toggleShowHideGroupItems() {
    var group = $(this);
    console.log('group clicked: ' + group.parent().attr('id') + '; display=' + group.next().css('display'));
    group.next().toggle();
    $(window).scrollTop(group.position().top);
}
