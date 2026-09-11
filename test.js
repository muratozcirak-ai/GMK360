
        $(document).ready(function() {
            // Populate Copy Floor Modal
            $('.btn-copy-floor').click(function() {
                var floorLevel = $(this).data('floorlevel');
                var floorName = $(this).data('floorname');
                $('#copyFloorModal #sourceFloorLevel').val(floorLevel);
                $('#copyFloorModal #sourceFloorNameDisplay').text(floorName);
                $('#copyFloorModal input[name="targetFloorLevels"]').val('');
            });
            // Populate Add Unit Modal when clicked from a specific floor
            var rowIdx = 1;
            $('#btnAddNewUnitRow').click(function() {
                var newRow = `<tr>
                                <td><input type="text" name="Units[${rowIdx}].DoorNumber" class="form-control form-control-sm" required /></td>
                                <td>
                                    <select name="Units[${rowIdx}].UnitStructure" class="form-select form-select-sm">
                                        <option value="Mesken" selected>Mesken (Normal Daire)</option>
                                        <option value="Çatı Dubleksi">Çatı Dubleksi</option>
                                        <option value="Ters Dubleks">Ters Dubleks</option>
                                <option value="Bahce Dubleksi">Bahçe Dubleksi</option>
                                        <option value="Bahçe Katı">Bahçe Katı</option>
                                        <option value="Dükkan / Ticari">Dükkan / Ticari</option>
                                    </select>
                                </td>
                                <td>
                                    <select name="Units[${rowIdx}].FacadeDirection" class="form-select form-select-sm">
                                        <option value="">Seçiniz...</option>
                                        <option value="Kuzey">Kuzey</option>
                                        <option value="Güney">Güney</option>
                                        <option value="Doğu">Doğu</option>
                                        <option value="Batı">Batı</option>
                                        <option value="Kuzeydoğu">Kuzeydoğu</option>
                                        <option value="Kuzeybatı">Kuzeybatı</option>
                                        <option value="Güneydoğu">Güneydoğu</option>
                                        <option value="Güneybatı">Güneybatı</option>
                                    </select>
                                </td>
                                <td><input type="number" name="Units[${rowIdx}].GrossSquareMeters" class="form-control form-control-sm" /></td>
                                <td><button type="button" class="btn btn-sm btn-danger remove-unit-row"><i class="bi bi-trash"></i></button></td>
                              </tr>`;
                $('#dynamicUnitsTable tbody').append(newRow);
                rowIdx++;
            });
            $(document).on('click', '.remove-unit-row', function() {
                $(this).closest('tr').remove();
            });
            $('.btn-add-unit-to-floor').click(function() {
                  var floorLevel = $(this).data('floorlevel');
                  var floorName = $(this).data('floorname');
                  $('[name="FloorLevel"]').val(floorLevel);
                  $('[name="FloorName"]').val(floorName);
                  $('#addUnitModal .modal-title').html('<i class="bi bi-plus-square text-success me-2"></i>' + floorName + ' - Hızlı Birim Ekle');
                  $('#floorDataRow').hide();
                  $('#addUnitAlert').hide();
              });
              $('[data-bs-target="#addUnitModal"]:not(.btn-add-unit-to-floor)').click(function() {
                  $('[name="FloorLevel"]').val('');
                  $('[name="FloorName"]').val('');
                  $('#addUnitModal .modal-title').html('<i class="bi bi-plus-square text-success me-2"></i>Yeni Kat / Birim Ekle');
                  $('#floorDataRow').show();
                  $('#addUnitAlert').show();
              });            
            // For the global Add Floor button
            $('.btn-add-global').click(function(){
                 $('#addUnitModalTitle').text('Yeni Kat / Birim Ekle');
                 // For a global add, we need them to type the floor manually, but we hid it!
                 // Let's just prompt them for floor level if it's empty
            });
            // Floor checkbox toggles all units in that floor// Floor checkbox toggles all units in that floor
            $('.select-all-floor').change(function() {
                var floor = $(this).data('floor');
                var isChecked = $(this).is(':checked');
                $('.unit-checkbox[data-floor="' + floor + '"]').prop('checked', isChecked);
            });
            // Bulk Edit Submit
            $('#btnSubmitBulkEdit').click(function() {
                var selectedIds = [];
                $('.unit-checkbox:checked').each(function() {
                    selectedIds.push($(this).val());
                });
                if (selectedIds.length === 0) {
                    alert('Lütfen düzenlemek için en az bir daire seçin.');
                    return;
                }
                $('#bulkUnitIds').val(selectedIds.join(','));
                // Set action and submit form
                $('#bulkEditForm').attr('action', '/ConstructionProject/BulkUpdateUnits?buildingId=@Model.Id');
                $('#bulkEditForm').submit();
            });
        });
    function toggleUnitCategory() {
        var cat = document.getElementById('unitCategorySelect').value;
        var parentWrapper = document.getElementById('parentUnitWrapper');
        var doorNumberLabel = document.getElementById('doorNumberLabel');
        
        if(doorNumberLabel) {
            if(cat.startsWith('OrtakAlan')) {
                doorNumberLabel.innerText = "Ortak Alan Adı (Örn: Hol, Şaft)";
            } else if(cat === 'Eklenti') {
                doorNumberLabel.innerText = "Eklenti Adı (Örn: Ayakkabılık, Depo)";
            } else {
                doorNumberLabel.innerText = "Kapı No / İsim";
            }
        }

        var floorRow = document.getElementById('floorDataRow');
        var structureWrapper = document.getElementById('unitStructureWrapper');
        if(cat === 'Eklenti') {
            parentWrapper.style.display = 'block';
        } else {
            parentWrapper.style.display = 'none';
        }
        if(cat === 'OrtakAlan_Blok') {
            floorRow.style.display = 'none';
        } else {
            floorRow.style.display = 'flex';
        }
        var ownershipWrapper = document.getElementById('ownershipWrapper');
        
        if(cat === 'Daire' || cat === 'Dükkan') {
            if(ownershipWrapper) ownershipWrapper.style.display = 'block';
        } else {
            if(ownershipWrapper) ownershipWrapper.style.display = 'none';
        }
        
        var facadeWrapper = document.getElementById('facadeWrapper');
        
        if(cat === 'Daire' || cat === 'Dükkan') {
            if(structureWrapper) structureWrapper.style.display = 'block';
            if(facadeWrapper) facadeWrapper.style.display = 'block';
        } else {
            if(structureWrapper) structureWrapper.style.display = 'none';
            if(facadeWrapper) facadeWrapper.style.display = 'none';
        }
    }

    // EDIT UNIT MODAL LOGIC
    $('.btn-edit-unit').click(function() {
        var id = $(this).data('unit-id');
        var door = $(this).data('door-number');
        var owner = $(this).data('owner-name');
        
        $('#editUnitId').val(id);
        $('#editDoorNumber').val(door);
        
        if(owner) {
            $('#editIsOwnedSwitch').prop('checked', true);
            $('#editOwnerName').val(owner).show();
        } else {
            $('#editIsOwnedSwitch').prop('checked', false);
            $('#editOwnerName').val('').hide();
        }
        
        var myModal = new bootstrap.Modal(document.getElementById('editUnitModal'));
        myModal.show();
    });

    $('#editIsOwnedSwitch').change(function() {
        if($(this).is(':checked')) {
            $('#editOwnerName').show().attr('required', true);
        } else {
            $('#editOwnerName').hide().removeAttr('required').val('');
        }
    });

    // ACCORDION STATE PERSISTENCE LOGIC
    $('.accordion-collapse').on('shown.bs.collapse', function (e) {
        localStorage.setItem('gmk_openAccordion', $(this).attr('id'));
    });
    
    var lastOpen = localStorage.getItem('gmk_openAccordion');
    if (lastOpen) {
        $('#' + lastOpen).addClass('show'); // Expand it immediately without animation
        
        // Try to scroll to it after rendering
        setTimeout(function() {
            var el = $('#' + lastOpen);
            if(el.length) {
                $('html, body').animate({ scrollTop: el.parent().offset().top - 80 }, 300);
            }
        }, 100);
    }

