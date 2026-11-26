#!/usr/bin/env python3
"""打印 Unity 场景/预制体的 UI 层级：名称、激活、锚点、坐标、尺寸、关键组件。"""
import re, sys

def load(path):
    txt = open(path, encoding='utf-8', errors='replace').read()
    docs = re.split(r'^--- !u!(\d+) &(\d+)(.*)$', txt, flags=re.M)
    D = {}
    for i in range(1, len(docs), 4):
        D[docs[i+1]] = {'cls': docs[i], 'body': docs[i+3]}
    return D

def f(b, k):
    m = re.search(r'^\s*' + re.escape(k) + r':\s*(.*)$', b, re.M)
    return m.group(1).strip() if m else None

def ridN(s):
    m = re.search(r'-?\d+', str(s) or '')
    return m.group(0) if m else '0'

def main():
    path = sys.argv[1]
    start = sys.argv[2] if len(sys.argv) > 2 else None
    depth = int(sys.argv[3]) if len(sys.argv) > 3 else 99
    D = load(path)

    names = {}
    for fid, d in D.items():
        if d['cls'] == '1':
            names[fid] = f(d['body'], 'm_Name')

    comp_go = {}
    go_comps = {}
    for fid, d in D.items():
        g = f(d['body'], 'm_GameObject')
        if g:
            gid = ridN(g)
            if gid != '0':
                comp_go[fid] = gid
                go_comps.setdefault(gid, []).append(fid)

    def root_tfs():
        out = []
        for fid, d in D.items():
            if d['cls'] == '224' and ridN(f(d['body'], 'm_Father')) == '0':
                out.append(fid)
        return out

    def walk(tf, ind):
        d = D.get(tf)
        if not d:
            return
        b = d['body']
        g = comp_go.get(tf, '0')
        nm = names.get(g, '?')
        act = f(D[g]['body'], 'm_IsActive') if g in D else '?'
        pos = f(b, 'm_AnchoredPosition')
        size = f(b, 'm_SizeDelta')
        amin = f(b, 'm_AnchorMin')
        amax = f(b, 'm_AnchorMax')
        sc = f(b, 'm_LocalScale')

        tags = []
        for c in go_comps.get(g, []):
            cls = D[c]['cls']
            cb = D[c]['body']
            if cls == '223':
                tags.append('Canvas(order=%s,ovr=%s,rm=%s)' % (
                    f(cb, 'm_SortingOrder'), f(cb, 'm_OverrideSorting'), f(cb, 'm_RenderMode')))
            elif cls == '225':
                tags.append('CG(a=%s,ray=%s,inter=%s)' % (
                    f(cb, 'm_Alpha'), f(cb, 'm_BlocksRaycasts'), f(cb, 'm_Interactable')))
            elif cls in ('114', '114'):
                pass
        for c in go_comps.get(g, []):
            cb = D[c]['body']
            if D[c]['cls'] == '114':
                gid = re.search(r'guid:\s*([0-9a-f]{32})', f(cb, 'm_Script') or '')
                tags.append('MB' + (':' + gid.group(1)[:6] if gid else ''))
            if D[c]['cls'] == '114' and 'm_Text' in cb:
                txt = f(cb, 'm_Text') or ''
                if txt:
                    tags.append('text="%s"' % txt)

        print('  ' * ind + '[%s] %s act=%s pos=%s size=%s amin=%s amax=%s scale=%s %s' % (
            g, nm, act, pos, size, amin, amax, sc, ' '.join(tags)))

        if ind >= depth:
            return
        ch = re.findall(r'- \{fileID: (-?\d+)\}', f(b, 'm_Children') or '')
        for c in ch:
            walk(c, ind + 1)

    roots = [start] if start else root_tfs()
    for r in roots:
        walk(r, 0)

if __name__ == '__main__':
    main()
