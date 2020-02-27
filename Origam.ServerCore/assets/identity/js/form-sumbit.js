
const form = document.querySelector("form");
const submitLinks = document.getElementsByClassName("submit-link");

Array.prototype.forEach.call(
    submitLinks, element => {
        element.addEventListener("click",function(event){
            const input = document.createElement("input");
            input.type = "hidden"
            input.name = "button"
            input.value = element.getAttribute("value");
            form.append(input)
            form.submit();
            event.preventDefault();
        });
});


