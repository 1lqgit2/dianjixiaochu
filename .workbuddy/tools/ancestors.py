import re, sys
txt = open(sys.argv[1], encoding='utf-8', errors='replace').read()
docs = re.split(r'^--- !u!(\d+) &(\d+)(.*)$', txt, flags=re.M)
D = {}
for i in range(1, len(docs), 4):
    D[docs[i+1]] = {'cls': docs[i], 'body': docs[i+3]}
def f(b, k):
    m = re.search(r'^\s*'+re.escape(k)+r':\s*(.*)$', b, re.M)
    return m.group(1).strip() if m else None
def rid(s):
    m = re.search(r'\d+', s or '')
    return m.group(0) if m else None
cur = sys.argv[2]; chain=[]
while cur and cur in D:
    d = D[cur]
    go = rid(f(d['body'],'m_GameObject'))
    name = f(D[go]['body'],'m_Name') if go and go in D and D[go]['cls']=='1' else '?'
    act = f(D[go]['body'],'m_IsActive') if go and go in D and D[go]['cls']=='1' else '?'
    comps=[]
    if go and go in D:
        for c in re.findall(r'component:\s*\{fileID:\s*(\d+)\}', D[go]['body']):
            cls = D[c]['cls'] if c in D else '?'
            if cls=='114':
                g=re.search(r'guid:\s*([0-9a-f]{32})', f(D[c]['body'],'m_Script') or '')
                comps.append('MB:'+ (g.group(1)[:6] if g else '?'))
            else:
                comps.append({'222':'CR','223':'Canvas','225':'CG','224':'RT','4':'T'}.get(cls,cls))
    chain.append(f"{cur} name={name} active={act} comps=[{','.join(comps)}]")
    cur = rid(f(d['body'],'m_Father'))
print('\n <- '.join(chain))
