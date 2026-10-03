import codecs
import re

path = 'GMK360.Web/Views/PhaseOne/Index.cshtml'
with codecs.open(path, 'r', 'utf-8-sig') as f:
    content = f.read()

# The regex matched and replaced everything up to the end of the button string.
# We need to make sure the foreach loop closes properly.
# Let's find: </form>\s*\}\s*</div>\s*</div>\s*</div>\s*</div>\s*\}\s*</div>\s*</td>
# And add </tr> } </tbody> </table> </div> </div> </div> and the closing brace for the outer foreach.

target = r'(<input type="hidden" name="budgetItemId" value="@item.Id" />\s*<button type="submit" class="btn btn-sm btn-outline-primary rounded-pill" style="font-size:0.75rem;">Fizibilite Seç</button>\s*</form>\s*\}\s*</div>\s*</div>\s*</div>\s*</div>\s*\})'
replacement = r'\1\n</div>\n</td>\n</tr>\n}\n}\n</tbody>\n</table>\n</div>\n</div>\n</div>\n'

content = re.sub(target, replacement, content)

with codecs.open(path, 'w', 'utf-8-sig') as f:
    f.write(content)