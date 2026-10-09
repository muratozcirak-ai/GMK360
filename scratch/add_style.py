with open(r'GMK360.Web\Views\Home\Index.cshtml', 'a', encoding='utf-8-sig') as f:
    f.write('''\n<style>
    .hover-lift { transition: transform 0.3s ease, box-shadow 0.3s ease; border: 1px solid transparent; }
    .hover-lift:hover { transform: translateY(-10px); box-shadow: 0 1rem 3rem rgba(0,0,0,.15)!important; border-color: #f97316; }
</style>\n''')
