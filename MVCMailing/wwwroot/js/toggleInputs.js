
let toggleables = document.getElementsByClassName('toggleable')

//there's only one toggle and wanna label it by class
let toggle = document.getElementsByClassName('toggle')[0];
    toggle.addEventListener('change', () => {
        for (let toggleable of toggleables)
        {
            toggleable.disabled = toggle.checked;
            toggleable.innerHTML = "";
        }
    })