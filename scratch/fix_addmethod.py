import codecs

path = 'GMK360.Web/Controllers/PhaseZeroController.cs'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

content = content.replace('        [HttpPost]\r\n        public async Task<IActionResult> AddFirmQuote', '        }\n\n        [HttpPost]\r\n        public async Task<IActionResult> AddFirmQuote')
content = content.replace('        [HttpPost]\n        public async Task<IActionResult> AddFirmQuote', '        }\n\n        [HttpPost]\n        public async Task<IActionResult> AddFirmQuote')

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)