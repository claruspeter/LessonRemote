
function objectifyForm(form) {
  var returnArray = {};
  const formArray = $(form).serializeArray();
  for (var i = 0; i < formArray.length; i++){
    returnArray[formArray[i]['name']] = formArray[i]['value'];
  }
  return returnArray;
}


$('.hijackMove').submit(function(ev) {
  ev.preventDefault();
  const data = objectifyForm(this);
  NavigateTo(data.move);
  this.reset();
  return false;
});

$('.hijackLink').click(function(ev){
  ev.preventDefault();
  NavigateTo($(this).attr('href'));
  return false;
})
