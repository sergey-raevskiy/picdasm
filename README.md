# picdasm

Decompiler for PIC18. Generates C-like pseudocode. Written in "flow state", quick and dirty.

Example output:

```c
void _FUNC_(int _pfn)
{
switch (_pfn) {

case 0x00000:
    goto _0x00018;

...

_0x0700C:
    INTCON &= ~(1 << GIE);
    WDTCON &= ~(1 << SWDTE);
    TRISC &= ~(1 << TRISC6);
    PORTC |= (1 << CK1);
    TRISC |= (1 << TRISC7);
    BSR = 0x05;
    Mem[BSR << 8 | 0x0F] = 0;
    Mem[BSR << 8 | 0x10] = 0;
    Mem[BSR << 8 | 0x11] = 0;
    Mem[BSR << 8 | 0x12] = 0;

    W = 0x00;

    W = Mem[BSR << 8 | 0x12] - W;
    if (!Z) goto _0x07036;

    W = 0x00;

    W = Mem[BSR << 8 | 0x11] - W;
    if (!Z) goto _0x07036;

    W = 0x9C;

    W = Mem[BSR << 8 | 0x10] - W;
    if (!Z) goto _0x07036;

    W = 0x40;

    W = Mem[BSR << 8 | 0x0F] - W;

...

} /* switch (_pfn) { */
}
```

