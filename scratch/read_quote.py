import io
filepath = r'GMK360.Core\Entities\B2B\B2BQuoteRequest.cs'
with io.open(filepath, 'r', encoding='utf-8') as f:
    print(f.read()[:500])
