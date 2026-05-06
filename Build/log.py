from sty import fg, bg, rs, ef


class Logger:
    steps = 0
    names: list[str]
    total_steps: int

    def __init__(self, names: list[str], total_steps: int):
        self.names = names
        self.total_steps = total_steps

        self.name_len = max([len(n) for n in names])

    def step(self, name: str, msg: str):
        name = name.rjust(self.name_len, " ")
        self.steps += 1

        pre = f"{fg.black}{bg.blue}{ef.bold} STEP {rs.all}"
        count = f"{fg.green}{bg.cyan}{ef.bold} {self.steps} of {fg.li_green}{self.total_steps} {rs.all}"

        print(f"{pre} {count}   {fg.green}{ef.bold}{name}: {fg.cyan}{msg}{rs.all}")
    
    def progress(self, msg: str):
        print(f"{ef.bold}{fg.blue}    >> {rs.all}{fg.white}{msg}{rs.all}")

    def exec(self, msg: str):
        print(f"{ef.dim}{fg.grey}    exec $ {rs.all}{fg.grey}{msg}{rs.all}")
    
    def fatal(self, msg: str):
        pre = f"{fg.black}{bg.red}{ef.bold} FATAL {rs.all}"

        print(f"{pre} {fg.red}{ef.bold}{msg}{rs.all}")
    
    def warn(self, msg: str):
        pre = f"{fg.black}{bg.yellow}{ef.bold} WARN {rs.all}"

        print(f"{pre} {fg.yellow}{ef.bold}{msg}{rs.all}")
    
    def info(self, msg: str):
        pre = f"{fg.black}{bg.green}{ef.bold} INFO {rs.all}"

        print(f"{pre} {fg.green}{ef.bold}{msg}{rs.all}")
    
    def debug(self, msg: str):
        print(f"{ef.dim}{msg}{rs.all}")
    
    def skip(self, num: int):
        self.steps += num

