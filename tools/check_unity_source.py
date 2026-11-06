"""Source/package checks only; this does not replace a licensed Unity build."""
from pathlib import Path
import json
import sys
from tree_sitter import Language, Parser
import tree_sitter_c_sharp
root=Path(__file__).resolve().parents[1]/'UnityProject'
parser=Parser(Language(tree_sitter_c_sharp.language()))
errors=[]
files=list(root.rglob('*.cs'))
for path in files:
    tree=parser.parse(path.read_bytes())
    nodes=[tree.root_node]
    while nodes:
        node=nodes.pop()
        if node.type=='ERROR' or node.is_missing:
            errors.append(f'{path.relative_to(root)}:{node.start_point.row+1}: syntax {node.type}')
        nodes.extend(node.children)
for path in list(root.rglob('*.asmdef'))+[root/'Packages/manifest.json']:
    try: json.loads(path.read_text())
    except Exception as exc: errors.append(f'{path}: {exc}')
names={json.loads(path.read_text())['name'] for path in root.rglob('*.asmdef')}
external={'UnityEngine.UI','Unity.Addressables','Unity.Addressables.Editor','Unity.ResourceManager','Unity.RenderPipelines.Core.Runtime','Unity.RenderPipelines.Universal.Runtime','UnityEngine.TestRunner','UnityEditor.TestRunner'}
for path in root.rglob('*.asmdef'):
    for reference in json.loads(path.read_text()).get('references',[]):
        if reference not in names|external: errors.append(f'{path}: unknown assembly reference {reference}')
# Catch serialized MonoBehaviours that Unity cannot map back to a matching MonoScript.
import re
for path in files:
    for name in re.findall(r'public\s+class\s+(\w+)\s*:\s*(?:MonoBehaviour|ScriptableObject|Graphic)',path.read_text()):
        if name!=path.stem: errors.append(f'{path}: {name} needs its own matching filename')
for error in errors:print(error,file=sys.stderr)
if errors:sys.exit(1)
print(f'PASS: {len(files)} C# files parsed; JSON, assembly references and serialized script names checked.')
print('Unity compilation, shaders, APK builds and device tests remain separate checks.')
