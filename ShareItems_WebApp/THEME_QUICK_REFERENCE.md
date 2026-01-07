# Dark Theme - Quick Reference

## ?? Color Palette

```
Background       #0b0f1a
Glass BG         rgba(255, 255, 255, 0.06)
Glass Border     rgba(255, 255, 255, 0.12)
Accent           #7c7cff
Glow             rgba(124, 124, 255, 0.35)
Text Primary     #e5e7eb
Text Muted       #9ca3af
Error            #ef4444
Success          #22c55e
```

---

## ??? Key Components

### Glass Card
```html
<div class="glass-card">
  Your content
</div>
```
**Effect**: Subtle blur with semi-transparent background

### Primary Button
```html
<button>Action</button>
```
**Color**: Purple (#7c7cff) with glow

### Danger Button
```html
<button class="destroy-btn">Delete</button>
```
**Color**: Red (#ef4444)

### Secondary Button
```html
<button class="cancel-btn">Cancel</button>
```
**Effect**: Glass background

---

## ?? Typography

```
H1  2.5em  #e5e7eb
H2  2.0em  #e5e7eb
H3  1.5em  #e5e7eb
P   1.0em  #e5e7eb

Muted text: #9ca3af
Links: #7c7cff
```

---

## ? Effects Applied

### Glassmorphism
- Cards and containers
- Navbar
- Modals
- Tables
- Input boxes

### Blur Intensity
- Light: 8px
- Medium: 10px
- Heavy: 12px (navbar)

### Accent Color Usage
- Primary buttons
- Active nav items
- Focus states
- Links
- Form focus glow

---

## ?? Responsive

**Desktop**: Full layout  
**Mobile (<768px)**: Stacked, 95% width

---

## ? Accessibility

? Focus states (purple outline)  
? Color contrast AAA  
? Keyboard navigation  
? Screen reader friendly  

---

## ?? What's New

**Added**:
- ? Dark solid background
- ? Glassmorphism effects
- ? Purple accent color
- ? Consistent design system
- ? Better focus states
- ? Responsive design

**Removed**:
- ? Background images
- ? Heavy gradients
- ? Bright neon colors
- ? Inconsistent styles

---

## ?? Files

```
wwwroot/
??? style.css (NEW THEME)
??? site.css (DUPLICATE)
??? style-backup.css (OLD THEME)
```

---

## ?? Usage Tips

1. **Use glass-card** for main content areas
2. **Use glass-container** for smaller sections
3. **Primary buttons** get accent color automatically
4. **Add class="muted"** for secondary text
5. **Forms** get glass styling automatically

---

## ?? Quick Customization

### Change Accent Color
```css
/* Find and replace */
#7c7cff ? #YOUR_COLOR
rgba(124, 124, 255, ...) ? rgba(...)
```

### Adjust Glass Opacity
```css
/* More visible */
rgba(255, 255, 255, 0.10)

/* More subtle */
rgba(255, 255, 255, 0.04)
```

### Change Blur
```css
backdrop-filter: blur(15px);  /* More blur */
backdrop-filter: blur(5px);   /* Less blur */
```

---

## ? Status

**Implementation**: ? Complete  
**Testing**: Manual test recommended  
**Breaking Changes**: None  
**Backend Changes**: None  

**Just refresh your browser to see the new theme!** ???
