#!/usr/bin/env python3
"""解析 Unity 场景 YAML，打印对象树 / 组件 / 脚本绑定。"""
import re, sys, os, glob

def load(path):
    txt = open(path, encoding='utf-8', errors='replace').read()
    docs = re.split(r'^--- !u!(\d+) &(\d+)(.*)$', txt, flags=re.M)
    out = {}
    # docs = [pre, cls, id, hdr, body, cls, id, hdr, body, ...]
    for i in range(1, len(docs), 4):
        cls, fid, hdr, body = docs[i], docs[i+1], docs[i+2], docs[i+3]
        out[fid] = {'cls': cls, 'hdr': hdr.strip(), 'body': body}
    return out

def field(body, key):
    m = re.search(r'^\s*' + re.escape(key) + r':\s*(.*)$', body, re.M)
    return m.group(1).strip() if m else None

def refs(body, key):
    m = re.search(r'^\s*' + re.escape(key) + r':\s*\n((?:\s+-\s*\{fileID:.*\n)+)', body, re.M)
    if not m: return []
    return re.findall(r'fileID:\s*(\d+)', m.group(1))

def build_guids(root):
    g = {}
    for meta in glob.glob(root + '/Assets/**/*.cs.meta', recursive=True):
        try:
            t = open(meta, encoding='utf-8', errors='replace').read()
        except: continue
        m = re.search(r'guid:\s*([0-9a-f]{32})', t)
        if m:
            g[m.group(1)] = meta[:-5].replace('\\', '/')
    return g

def main(scene, focus=None, depth=99):
    docs = load(scene)
    guids = build_guids(os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(scene)))))

    go_of = {}      # component id -> gameobject id
    go_name = {}
    go_active = {}
    go_comps = {}
    for fid, d in docs.items():
        if d['cls'] == '1':
            go_name[fid] = field(d['body'], 'm_Name')
            go_active[fid] = field(d['body'], 'm_IsActive')
            go_comps[fid] = refs(d['body'], 'm_Component') or re.findall(r'component:\s*\{fileID:\s*(\d+)\}', d['body'])
        if d['cls'] in ('4', '224'):
            g = field(d['body'], 'm_GameObject')
            if g:
                go_of[re.search(r'\d+', g).group(0)] = None
    # map component -> go via m_GameObject
    comp_go = {}
    for fid, d in docs.items():
        if d['cls'] in ('4', '224', '114', '222', '223', '225', '95'):
            g = field(d['body'], 'm_GameObject')
            if g:
                comp_go[fid] = re.search(r'\d+', g).group(0)

    def tf_of_go(go):
        for c in go_comps.get(go, []):
            if docs.get(c, {}).get('cls') in ('4', '224'):
                return c
        return None

    def comp_desc(cid):
        d = docs.get(cid)
        if not d: return '?'
        cls = d['cls']
        if cls == '114':
            s = field(d['body'], 'm_Script') or ''
            gm = re.search(r'guid:\s*([0-9a-f]{32})', s)
            path = guids.get(gm.group(1), 'UNKNOWN') if gm else '?'
            return 'MB:' + os.path.basename(path)
        return {'222': 'CanvasRenderer', '223': 'Canvas', '225': 'CanvasGroup',
                '4': 'Transform', '224': 'RectTransform'}.get(cls, 'cls' + cls)

    def walk(tf, ind=0, maxd=99):
        g = comp_go.get(tf)
        if not g: return
        name = go_name.get(g, '?')
        act = go_active.get(g, '?')
        canvas = field(docs[tf]['body'], 'm_Scale') if False else None
        comps = ', '.join(comp_desc(c) for c in go_comps.get(g, []))
        print('  ' * ind + f'[{g}] {name}  active={act}  ({comps})')
        if ind >= maxd: return
        for ch in refs(docs[tf]['body'], 'm_Children'):
            walk(ch, ind + 1, maxd)

    if focus:
        walk(focus, 0, depth)
    else:
        for fid, d in docs.items():
            if d['cls'] in ('4', '224') and field(d['body'], 'm_Father') == '{fileID: 0}':
                walk(fid, 0, depth)

if __name__ == '__main__':
    scene = sys.argv[1]
    focus = sys.argv[2] if len(sys.argv) > 2 else None
    depth = int(sys.argv[3]) if len(sys.argv) > 3 else 99
    main(scene, focus, depth)
