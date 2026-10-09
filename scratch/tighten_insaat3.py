import re

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'r', encoding='utf-8-sig', errors='ignore') as f:
    content = f.read()

# Tighten the cards
# Replace <div class="card-body p-4"> with <div class="card-body p-3">
content = content.replace('<div class="card-body p-4">', '<div class="card-body p-3">')
# Replace <div class="row g-4"> with <div class="row g-3">
content = content.replace('<div class="row g-4">', '<div class="row g-3">')
# Replace mb-4 with mb-3 in certain places inside the cards
# Specifically around the text sections
content = content.replace('<div class="row mb-4">', '<div class="row mb-2">')
content = content.replace('<h6 class="fw-bold border-bottom pb-2 mb-3">', '<h6 class="fw-bold border-bottom pb-1 mb-2">')
content = content.replace('<div class="mt-4 pt-3 border-top position-relative">', '<div class="mt-3 pt-2 border-top position-relative">')
content = content.replace('<div class="row mt-4">', '<div class="row mt-3">')

with open(r'GMK360.Web\Views\ConstructionProject\Details.cshtml', 'w', encoding='utf-8-sig') as f:
    f.write(content)
