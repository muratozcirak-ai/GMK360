import io
import re

filepath = r'GMK360.Web\Views\PhaseZero\Index.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Replace the verifyPinAndAccept JS logic
old_logic = """        function verifyPinAndAccept() {
            let pin = Array.from(document.querySelectorAll('.pin-digit')).map(i => i.value).join('');
            if(pin.length < 6) {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Lütfen 6 haneli şifreyi eksiksiz girin.</span>";
                document.getElementById('pinErrorText').style.display = 'block';
                return;
            }
            
            // Simüle edilmiş PIN onayı (Gerçek senaryoda backend'e gider)
            if(pin !== '123456') {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Hatalı PIN! (Demo için: 123456)</span>";
                document.getElementById('pinErrorText').style.display = 'block';
                return;
            }
            
            // Onaylandı, sayfayı reload yap
            alert("Teklif başarıyla onaylandı ve maliyet olarak işlendi!");
            location.reload();
        }"""

new_logic = """        function verifyPinAndAccept() {
            let pin = Array.from(document.querySelectorAll('.pin-digit')).map(i => i.value).join('');
            if(pin.length < 6) {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Lütfen 6 haneli şifreyi eksiksiz girin.</span>";
                document.getElementById('pinErrorText').style.display = 'block';
                return;
            }
            
            let formData = new FormData();
            formData.append('docId', currentPinDocId);
            formData.append('pin', pin);
            
            fetch('/api/PhaseZeroPin/ApproveQuote', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json().then(data => ({status: res.status, body: data})))
            .then(obj => {
                if(obj.status === 200 && obj.body.success) {
                    var modal = bootstrap.Modal.getInstance(document.getElementById('pinApprovalModal'));
                    modal.hide();
                    
                    // Başarı mesajı ve yenileme
                    const toastDiv = document.createElement('div');
                    toastDiv.className = 'position-fixed bottom-0 end-0 p-3';
                    toastDiv.style.zIndex = '1100';
                    toastDiv.innerHTML = `
                        <div class="toast align-items-center text-white bg-success border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
                          <div class="d-flex">
                            <div class="toast-body fw-bold fs-6">
                              <i class="bi bi-check-circle-fill me-2"></i> Teklif Onaylandı! Maliyet işleniyor...
                            </div>
                          </div>
                        </div>
                    `;
                    document.body.appendChild(toastDiv);
                    
                    setTimeout(() => location.reload(), 1500);
                } else {
                    document.getElementById('pinErrorText').innerHTML = `<span class='text-danger fw-bold'>${obj.body.message || 'Hatalı PIN! Veya işlem gerçekleştirilemedi.'}</span>`;
                    document.getElementById('pinErrorText').style.display = 'block';
                }
            })
            .catch(err => {
                document.getElementById('pinErrorText').innerHTML = "<span class='text-danger fw-bold'>Hatalı PIN! (Demo için: 123456)</span>";
                document.getElementById('pinErrorText').style.display = 'block';
            });
        }"""

content = content.replace(old_logic, new_logic)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print("Injected actual PIN API Call")
