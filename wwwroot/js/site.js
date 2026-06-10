document.addEventListener("DOMContentLoaded", () => {
  document.querySelectorAll(".js-confirm").forEach((button) => {
    button.addEventListener("click", (event) => {
      const message = button.dataset.confirm || "Czy na pewno?";
      if (!window.confirm(message)) {
        event.preventDefault();
      }
    });
  });

  const dateInput = document.getElementById("slotDate");
  const slotSelect = document.getElementById("slotSelect");
  const slotHint = document.getElementById("slotHint");

  if (dateInput && slotSelect) {
    dateInput.addEventListener("change", async () => {
      const serviceId = dateInput.dataset.serviceId;
      const day = dateInput.value;
      const url = `/Services/Slots?serviceId=${encodeURIComponent(serviceId)}&day=${encodeURIComponent(day)}`;

      slotSelect.disabled = true;
      slotHint.textContent = "Ladowanie wolnych terminow...";

      try {
        const response = await fetch(url);
        const slots = await response.json();
        slotSelect.innerHTML = '<option value="">Wybierz termin</option>';

        slots.forEach((slot) => {
          const option = document.createElement("option");
          option.value = slot.id;
          option.textContent = slot.label;
          slotSelect.appendChild(option);
        });

        slotHint.textContent = slots.length
          ? "Wybierz jeden z dostepnych terminow."
          : "Brak wolnych terminow w wybranym dniu.";
      } catch {
        slotHint.textContent = "Nie udalo sie pobrac terminow. Sprobuj ponownie.";
      } finally {
        slotSelect.disabled = false;
      }
    });
  }
});
