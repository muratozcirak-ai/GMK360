import re

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

razor_old_sub_block = '''<div class="sub-blocks-container mt-4 pt-3 border-top border-danger border-opacity-25" style="display: none;">
                                                    <div class="row">
                                                        <div class="col-12 col-lg-4">
                                                            <label class="form-label small fw-bold text-danger sub-block-label">Kaç Kule / Blok Var?</label>
                                                            <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'ExistingBlocks', @i)" />
                                                        </div>
                                                    </div>
                                                    <div class="sub-blocks-list"></div>
                                                </div>
                                            </div>
                                        </div>'''

razor_new_sub_block = '''<div class="sub-blocks-container mt-4 pt-3 border-top border-primary border-opacity-25" style="display: none;">
                                                    <div class="row">
                                                        <div class="col-12 col-lg-4">
                                                            <label class="form-label small fw-bold text-primary sub-block-label">Kaç Kule / Blok Var?</label>
                                                            <input type="number" class="form-control form-control-sm" min="0" max="10" value="0" oninput="generateSubBlocks(this, 'TargetBlocks', @i)" />
                                                        </div>
                                                    </div>
                                                    <div class="sub-blocks-list"></div>
                                                </div>
                                            </div>
                                        </div>'''


content = re.sub(r'</div>\s*</div>\s*</div>\s*</div>\s*i\+\+;', razor_old_sub_block + '\n                                        i++;', content, count=1)
content = re.sub(r'</div>\s*</div>\s*</div>\s*</div>\s*i\+\+;', razor_new_sub_block + '\n                                        i++;', content, count=1)

with open('GMK360.Web/Views/ConstructionProject/Create.cshtml', 'w', encoding='utf-8') as f:
    f.write(content)
print("Razor UI updated!")
