import io
import re

filepath = r'GMK360.Web\Views\ConstructionProject\Details.cshtml'
with io.open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# The UI usually has a sidebar or grid of cards for Project modules. Let's find where PhaseZero is linked.
# Search for something like: <a href="/PhaseZero/Index/@Model.Id"
link_injection = """
                    <!-- PUANTAJ MODÜLÜ KARTI -->
                    <div class="col-md-3 mb-4">
                        <div class="card h-100 border-0 shadow-sm hover-shadow transition-all rounded-4">
                            <div class="card-body text-center p-4">
                                <div class="bg-primary bg-opacity-10 rounded-circle d-inline-flex p-3 mb-3">
                                    <i class="bi bi-calendar-check fs-2 text-primary"></i>
                                </div>
                                <h5 class="fw-bold text-dark">Şantiye Puantaj</h5>
                                <p class="text-muted small">Günlük Usta/Personel Yoklaması</p>
                                <a href="/DailyTimesheet/Project/@Model.Id" target="_blank" class="btn btn-outline-primary rounded-pill w-100 fw-bold">
                                    Puantajı Aç <i class="bi bi-box-arrow-up-right ms-1"></i>
                                </a>
                            </div>
                        </div>
                    </div>
"""

# Let's insert it before the last </div> in the row of cards. We can just look for the PhaseZero card and append after its parent col-md-3.
pattern = r'(<div class="col-md-3[^>]*>.*?href="/PhaseZero/Index.*?</div>\s*</div>)'
# Actually let's just find the row that contains these cards.
content = re.sub(pattern, r'\1' + '\n' + link_injection, content, flags=re.DOTALL)

with io.open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)
print("Injected Puantaj Link")
