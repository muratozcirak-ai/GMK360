import codecs

path = 'GMK360.Web/Views/ConstructionProject/Details.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

modal_html = '''
<!-- Edit Stakeholder Modal -->
<div class="modal fade" id="editStakeholderModal" tabindex="-1">
    <div class="modal-dialog modal-dialog-centered">
        <div class="modal-content border-warning border-2 rounded-4 shadow-lg">
            <div class="modal-header bg-warning bg-opacity-10 border-0">
                <h5 class="modal-title fw-bold text-dark"><i class="bi bi-pencil-square me-2"></i> Paydaş Düzenle</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <form asp-action="EditStakeholder" asp-controller="ConstructionProject" method="post">
                <div class="modal-body p-4">
                    <input type="hidden" name="stakeholderId" id="editStakeholderId" />
                    
                    <div class="mb-4">
                        <label class="form-label fw-bold text-secondary small text-uppercase">Projedeki Rolü</label>
                        <select name="role" id="editStakeholderRole" class="form-select form-select-lg bg-light border-0" required>
                            <option value="Landowner">Arsa Sahibi / Hak Sahibi</option>
                            <option value="Representative">Temsilci / Avukat</option>
                            <option value="Consultant">Müşavir / Takipçi</option>
                            <option value="Other">Diğer (Mirasçı vb.)</option>
                        </select>
                    </div>

                    <div class="mb-4">
                        <label class="form-label fw-bold text-secondary small text-uppercase">Hisse / Arsa Payı (%)</label>
                        <div class="input-group input-group-lg">
                            <span class="input-group-text bg-light border-0"><i class="bi bi-pie-chart text-muted"></i></span>
                            <input type="number" step="0.01" name="sharePercentage" id="editStakeholderShare" class="form-control bg-light border-0" placeholder="Örn: 25.50">
                            <span class="input-group-text bg-light border-0 text-muted">%</span>
                        </div>
                    </div>
                </div>
                <div class="modal-footer border-0 bg-light rounded-bottom-4">
                    <button type="button" class="btn btn-outline-secondary px-4 fw-bold rounded-pill" data-bs-dismiss="modal">İptal</button>
                    <button type="submit" class="btn btn-warning px-4 fw-bold rounded-pill shadow-sm"><i class="bi bi-check2-circle me-2"></i>Değişiklikleri Kaydet</button>
                </div>
            </form>
        </div>
    </div>
</div>

<script>
    function editStakeholder(id, role, share) {
        document.getElementById('editStakeholderId').value = id;
        document.getElementById('editStakeholderRole').value = role;
        document.getElementById('editStakeholderShare').value = share;
        var myModal = new bootstrap.Modal(document.getElementById('editStakeholderModal'));
        myModal.show();
    }
</script>
'''

if 'id="editStakeholderModal"' not in content:
    content = content.replace('<!-- Add Stakeholder Modal -->', modal_html + '\n  <!-- Add Stakeholder Modal -->')
    with codecs.open(path, 'w', 'utf-8-sig') as f:
        f.write(content)
    print("Added Edit Modal!")
else:
    print("Modal already exists!")