#!/usr/bin/env python3
"""
Generate ISM Sponsor app icons with proper branding
"""

try:
    from PIL import Image, ImageDraw, ImageFont
    PIL_AVAILABLE = True
except ImportError:
    PIL_AVAILABLE = False
    print("PIL not available, creating simple colored icons...")

import os

# ISM Brand Colors
ISM_GREEN = (13, 95, 59)  # #0d5f3b
ISM_YELLOW = (244, 197, 66)  # #f4c542
WHITE = (255, 255, 255)

def create_simple_icon(size, output_path):
    """Create a simple colored icon without text (fallback)"""
    if PIL_AVAILABLE:
        # Create image with ISM green background
        img = Image.new('RGB', (size, size), ISM_GREEN)
        draw = ImageDraw.Draw(img)
        
        # Draw a simple "ISM" lettermark in the center
        # Create a rounded rectangle
        margin = size // 6
        draw.rounded_rectangle(
            [margin, margin, size - margin, size - margin],
            radius=size // 10,
            fill=ISM_YELLOW,
            outline=WHITE,
            width=max(2, size // 50)
        )
        
        # Save the icon
        img.save(output_path, 'PNG', optimize=True)
        print(f"Created icon: {output_path} ({size}x{size})")
        return True
    return False

def create_icon_with_text(size, output_path):
    """Create an icon with ISM text"""
    if not PIL_AVAILABLE:
        return False
    
    # Create image with ISM green background
    img = Image.new('RGB', (size, size), ISM_GREEN)
    draw = ImageDraw.Draw(img)
    
    try:
        # Try to use a system font
        font_size = size // 3
        font = ImageFont.truetype("/System/Library/Fonts/Helvetica.ttc", font_size)
    except:
        # Fallback to default font
        font = ImageFont.load_default()
    
    # Draw "ISM" text in center
    text = "ISM"
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    position = ((size - text_width) // 2, (size - text_height) // 2 - size // 20)
    
    # Draw text with shadow for depth
    shadow_offset = max(2, size // 100)
    draw.text((position[0] + shadow_offset, position[1] + shadow_offset), text, fill=(0, 0, 0, 128))
    draw.text(position, text, fill=WHITE, font=font)
    
    # Add a subtle border
    border_width = max(1, size // 100)
    draw.rectangle([0, 0, size - 1, size - 1], outline=ISM_YELLOW, width=border_width)
    
    # Save the icon
    img.save(output_path, 'PNG', optimize=True)
    print(f"Created icon with text: {output_path} ({size}x{size})")
    return True

def main():
    # Get the icons directory
    script_dir = os.path.dirname(os.path.abspath(__file__))
    icons_dir = os.path.join(os.path.dirname(script_dir), 'wwwroot', 'icons')
    
    # Create icons directory if it doesn't exist
    os.makedirs(icons_dir, exist_ok=True)
    
    # Icon sizes to generate
    sizes = [192, 512]
    
    if PIL_AVAILABLE:
        print("PIL is available, generating high-quality icons...")
        for size in sizes:
            output_path = os.path.join(icons_dir, f'icon-{size}.png')
            # Backup old icon
            if os.path.exists(output_path):
                os.rename(output_path, output_path + '.old')
            
            # Try with text first, fallback to simple
            if not create_icon_with_text(size, output_path):
                create_simple_icon(size, output_path)
    else:
        print("=" * 60)
        print("PIL (Pillow) is not installed.")
        print("To generate proper icons, install it with:")
        print("  pip3 install Pillow")
        print("")
        print("Alternatively, you can:")
        print("1. Create icons manually using a design tool")
        print("2. Use an online icon generator")
        print("3. Install imagemagick: brew install imagemagick")
        print("=" * 60)
        return 1
    
    print("\nIcons generated successfully!")
    print(f"Location: {icons_dir}")
    return 0

if __name__ == '__main__':
    exit(main())
