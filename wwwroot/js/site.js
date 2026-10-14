document.querySelectorAll("img").forEach(image => {
  const fallback = () => {
    image.hidden = true;
    const label = image.parentElement.querySelector(".image-fallback");
    if (label) label.hidden = false;
  };
  image.addEventListener("error", fallback);
  if (image.complete && image.naturalWidth === 0) fallback();
});
document.querySelectorAll("form[data-confirm]").forEach(form => {
  form.addEventListener("submit", event => {
    if (!window.confirm(form.dataset.confirm)) event.preventDefault();
  });
});
