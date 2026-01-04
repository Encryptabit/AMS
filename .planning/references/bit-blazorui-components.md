# Bit.BlazorUI Component Reference

This document contains detailed usage documentation for all Bit.BlazorUI components.

**Package:** `Bit.BlazorUI` (v10.3.0)
**Documentation Source:** https://blazorui.bitplatform.dev/components/

---

## Table of Contents

### Buttons
- [ActionButton](#actionbutton)
- [Button](#button)
- [ButtonGroup](#buttongroup)
- [MenuButton](#menubutton)
- [ToggleButton](#togglebutton)

### Inputs
- [Calendar](#calendar)
- [Checkbox](#checkbox)
- [ChoiceGroup](#choicegroup)
- [Dropdown](#dropdown)
- [FileUpload](#fileupload)
- [NumberField](#numberfield)
- [OtpInput](#otpinput)
- [Rating](#rating)
- [SearchBox](#searchbox)
- [Slider](#slider)
- [TextField](#textfield)
- [Toggle](#toggle)

### Pickers
- [DatePicker](#datepicker)
- [TimePicker](#timepicker)
- [DateRangePicker](#daterangepicker)
- [CircularTimePicker](#circulartimepicker)
- [ColorPicker](#colorpicker)

### Layouts
- [Footer](#footer)
- [Grid](#grid)
- [Header](#header)
- [Layout](#layout)
- [Spacer](#spacer)
- [Stack](#stack)

### Lists
- [BasicList](#basiclist)
- [Carousel](#carousel)
- [Swiper](#swiper)
- [Timeline](#timeline)

### Navs
- [Breadcrumb](#breadcrumb)
- [DropMenu](#dropmenu)
- [Nav](#nav)
- [NavBar](#navbar)
- [Pagination](#pagination)
- [Pivot](#pivot)

### Notifications
- [Badge](#badge)
- [Message](#message)
- [Persona](#persona)
- [SnackBar](#snackbar)
- [Tag](#tag)

### Progress
- [Loading](#loading)
- [Progress](#progress)
- [Shimmer](#shimmer)

### Surfaces
- [Accordion](#accordion)
- [Callout](#callout)
- [Card](#card)
- [Collapse](#collapse)
- [Dialog](#dialog)
- [Modal](#modal)
- [Panel](#panel)
- [ScrollablePane](#scrollablepane)
- [Splitter](#splitter)
- [Tooltip](#tooltip)

### Utilities
- [Element](#element)
- [Icon](#icon)
- [Image](#image)
- [Label](#label)
- [Link](#link)
- [MediaQuery](#mediaquery)
- [Overlay](#overlay)
- [PullToRefresh](#pulltorefresh)
- [Separator](#separator)
- [Sticky](#sticky)
- [SwipeTrap](#swipetrap)
- [Text](#text)

### Extras
- [Iconography](#iconography)
- [Theming](#theming)

---

# Buttons

## ActionButton

The **ActionButton** is a lightweight and specialized button/link component with icon-first styling, built-in loading states, size presets, and colorized text/icon support. It combines button functionality with optional link behavior for flexible UI interactions.

### Component Overview

BitActionButton is designed for:
- Icon-first action buttons with optional text labels
- Convertible button-to-link elements
- Loading state indicators with customizable spinners
- Accessible form submissions and resets
- Color-coded status indicators
- RTL layout support

### Properties & Parameters

#### Core ActionButton Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AllowDisabledFocus` | bool | false | Allows focus on disabled buttons (accessibility) |
| `AriaDescription` | string? | null | Screen reader description for the button |
| `ButtonType` | BitButtonType | null | Button behavior type: Button, Submit, or Reset |
| `ChildContent` / `Body` | RenderFragment? | null | Custom button content/children |
| `Classes` | BitActionButtonClassStyles? | null | CSS class customization for Root, Icon, Content, Spinner |
| `Color` | BitColor? | null | Colorizes icon and text (Primary, Secondary, Success, Error, etc.) |
| `FullWidth` | bool | false | Stretches button to full available width |
| `Href` | string? | null | Converts button to hyperlink when specified |
| `IconName` | string? | null | Icon identifier to display |
| `IconOnly` | bool | false | Hides text label, shows icon only |
| `IconPosition` | BitIconPosition | null | Icon placement: Start (before text) or End (after text) |
| `IsLoading` | bool | false | Displays loading state with spinner |
| `LoadingTemplate` | RenderFragment? | null | Custom template for loading indicator |
| `OnClick` | EventCallback<MouseEventArgs> | — | Click event handler |
| `Rel` | BitLinkRels? | null | Link relationship attributes (for Href usage) |
| `Size` | BitSize? | null | Preset dimensions: Small, Medium, or Large |
| `Target` | string? | null | Link target (_blank, _self, _parent, _top) |
| `Title` | string? | null | Tooltip text on hover |
| `Underlined` | bool | false | Underlines text for link-styled appearance |

#### Inherited Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AriaLabel` | string? | null | Accessible name for screen readers |
| `Class` | string? | null | Additional CSS classes |
| `Dir` | BitDir? | null | Text direction: Ltr, Rtl, or Auto |
| `HtmlAttributes` | Dictionary | empty | Custom HTML attributes |
| `Id` | string? | null | HTML element ID |
| `IsEnabled` | bool | true | Enables/disables the button |
| `Style` | string? | null | Inline CSS styles |
| `TabIndex` | string? | null | Tab order index |
| `Visibility` | BitVisibility | Visible | Visibility state: Visible, Hidden, or Collapsed |

### Enum Values

**BitButtonType** - Button behavior
- `Button` - Standard clickable button
- `Submit` - Form submission button
- `Reset` - Form reset button

**BitColor** - Icon and text coloring
- `Primary`, `Secondary`, `Tertiary`
- `Info`, `Success`, `Warning`, `SevereWarning`, `Error`
- `PrimaryBackground`, `SecondaryBackground`, `TertiaryBackground`
- `PrimaryForeground`, `SecondaryForeground`, `TertiaryForeground`
- `PrimaryBorder`, `SecondaryBorder`, `TertiaryBorder`

**BitSize** - Preset dimensions
- `Small` - Compact sizing
- `Medium` - Standard sizing
- `Large` - Expanded sizing

**BitIconPosition** - Icon placement
- `Start` - Icon before text (default)
- `End` - Icon after text

**BitLinkRels** - Link relationship attributes
- `Alternate`, `Author`, `Bookmark`, `External`
- `Help`, `License`, `Next`, `NoFollow`
- `NoOpener`, `NoReferrer`, `Prev`, `Search`, `Tag`

**BitDir** - Text direction
- `Ltr` - Left-to-right
- `Rtl` - Right-to-left
- `Auto` - Auto-detect

**BitVisibility** - Visibility control
- `Visible` - Normal display
- `Hidden` - Hidden but takes up space
- `Collapsed` - Hidden with no space allocation

### CSS Customization

The `Classes` property accepts `BitActionButtonClassStyles` for granular styling control:

| Class Property | Element | Purpose |
|---|---|---|
| `Root` | Root button/link element | Apply styles to entire component |
| `Icon` | Icon element | Style the icon appearance |
| `Content` | Content container | Style text and content wrapper |
| `Spinner` | Loading spinner | Customize loading indicator appearance |

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnClick` | EventCallback<MouseEventArgs> | Fires when button is clicked |

### Code Examples

#### Basic Action Button with Icon

```razor
<BitActionButton IconName="add" Title="Create new item">
    Add Item
</BitActionButton>
```

#### Icon-Only Button

```razor
<BitActionButton 
    IconName="delete" 
    IconOnly="true"
    Color="BitColor.Error"
    Title="Delete" />
```

#### Loading State Button

```razor
@code {
    private bool isLoading = false;

    private async Task HandleSubmit()
    {
        isLoading = true;
        await Task.Delay(2000);
        isLoading = false;
    }
}

<BitActionButton 
    OnClick="HandleSubmit"
    IsLoading="isLoading"
    ButtonType="BitButtonType.Submit">
    Submit Form
</BitActionButton>
```

#### Styled Buttons with Different Sizes & Colors

```razor
<BitActionButton 
    IconName="check" 
    Color="BitColor.Success"
    Size="BitSize.Small">
    Approve
</BitActionButton>

<BitActionButton 
    IconName="warning" 
    Color="BitColor.Warning"
    Size="BitSize.Medium">
    Attention Required
</BitActionButton>

<BitActionButton 
    IconName="error" 
    Color="BitColor.Error"
    Size="BitSize.Large">
    Critical Error
</BitActionButton>
```

#### Link-Style Navigation Button

```razor
<BitActionButton 
    Href="/dashboard" 
    IconName="home"
    Target="_self"
    Underlined="true">
    Go to Dashboard
</BitActionButton>

<BitActionButton 
    Href="https://external-site.com" 
    IconName="open_in_new"
    Rel="BitLinkRels.External | BitLinkRels.NoOpener"
    Target="_blank">
    External Link
</BitActionButton>
```

#### Custom Loading Template

```razor
<BitActionButton 
    IsLoading="isProcessing"
    LoadingTemplate="@customLoader">
    Process Data
</BitActionButton>

@* Define custom loader template *@
<RenderFragment @ref="customLoader">
    <span>Processing... (custom spinner here)</span>
</RenderFragment>
```

#### Accessibility Features

```razor
<BitActionButton 
    Id="save-btn"
    AriaLabel="Save document"
    AriaDescription="Click to save the current document to the server"
    AllowDisabledFocus="true"
    IsEnabled="!isFormInvalid">
    Save
</BitActionButton>
```

#### RTL Support

```razor
<BitActionButton 
    IconName="delete" 
    Dir="BitDir.Rtl"
    Color="BitColor.Error">
    حذف (Delete in Arabic)
</BitActionButton>
```

#### Full Width Button

```razor
<BitActionButton 
    FullWidth="true"
    ButtonType="BitButtonType.Submit"
    Size="BitSize.Large">
    Complete Purchase
</BitActionButton>
```

#### Custom CSS Styling

```razor
<BitActionButton 
    IconName="favorite" 
    Color="BitColor.Primary"
    Classes="@(new BitActionButtonClassStyles
    {
        Root = "custom-action-btn",
        Icon = "custom-icon-style",
        Content = "custom-content-style"
    })" />
```

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| `UniqueId` | Guid | Unique identifier for the component instance |
| `RootElement` | ElementReference | Direct reference to the HTML root element for JS interop |

### Key Features

- **Icon-First Design**: Primary visual focus on icons with optional text
- **Dual Mode**: Functions as button or link depending on `Href` property
- **Loading States**: Built-in spinner display with custom template support
- **Color Coding**: Semantic colors for status indication
- **Accessibility**: Full ARIA support with screen reader descriptions
- **Responsive Sizing**: Small, Medium, Large presets
- **Link Attributes**: Complete control over link behavior (target, rel, underline)
- **RTL Support**: Bidirectional text layout support
- **Form Integration**: Submit and Reset button types for form handling

---

I've successfully documented the Bit.BlazorUI ActionButton component with a comprehensive markdown section that includes:

1. **Component Overview** - Description of purpose and use cases
2. **Properties Table** - All core properties and inherited properties with types, defaults, and descriptions
3. **Enum Values** - Complete enum reference for BitButtonType, BitColor, BitSize, BitIconPosition, BitLinkRels, BitDir, and BitVisibility
4. **CSS Customization** - Details on the BitActionButtonClassStyles customization options
5. **Events** - OnClick event callback
6. **7 Real-World Code Examples**:
   - Basic button with icon
   - Icon-only button
   - Loading state with async handling
   - Multiple styled buttons with different colors and sizes
   - Link-style navigation (internal and external)
   - Custom loading template
   - Accessibility features
   - RTL support
   - Full width button
   - CSS customization

7. **Public Members** - UniqueId and RootElement references
8. **Key Features** - Summary of capabilities

The documentation is ready to be appended to your component reference file!

---

## Button

The **BitButton** component enables users to take actions with a single tap. It's a versatile UI element commonly used in forms, dialogs, and navigation contexts, supporting multiple variants, sizes, colors, and interactive states.

### Component Overview

BitButton is a fully customizable button component that extends beyond standard HTML button functionality with support for loading states, icons, async operations, and floating positioning. It's accessible by default with ARIA support and integrates seamlessly with Blazor's event handling and data binding.

### Properties/Parameters

#### Content & Display

| Property | Type | Description |
|----------|------|-------------|
| `ChildContent` | `RenderFragment` | Main button content (typically text or inline elements) |
| `PrimaryTemplate` | `RenderFragment` | Alternative template for primary content area |
| `SecondaryText` | `string` | Optional secondary text displayed alongside primary content |
| `SecondaryTemplate` | `RenderFragment` | Alternative template for secondary content area |
| `IconName` | `string` | Name of icon to display (uses platform icon set) |
| `IconUrl` | `string` | URL to custom icon image |

#### Styling & Appearance

| Property | Type | Description |
|----------|------|-------------|
| `Variant` | `BitVariant` | Button visual style (Fill, Outline, Text) - default: Fill |
| `Color` | `BitColor` | Semantic or neutral color - default: Primary |
| `Size` | `BitSize` | Button dimensions (Small, Medium, Large) - default: Medium |
| `FullWidth` | `bool` | Expand button to 100% of container width - default: false |
| `Class` | `string` | Additional CSS classes for custom styling |
| `Style` | `string` | Inline CSS styles |

#### State Management

| Property | Type | Description |
|----------|------|-------------|
| `IsLoading` | `bool` | Toggles button loading state with spinner animation |
| `LoadingLabel` | `string` | Text to display while button is in loading state |
| `IsDisabled` | `bool` | Disables button interaction - default: false |
| `AutoLoading` | `bool` | Automatically set loading state during async operations - default: true |
| `Reclickable` | `bool` | Allow clicking button while in loading state - default: false |

#### Navigation & Linking

| Property | Type | Description |
|----------|------|-------------|
| `Href` | `string` | URL for navigation; converts button to anchor element |
| `Target` | `string` | Link target attribute (_blank, _self, etc.) |
| `Rel` | `string` | Link relation attribute (noopener, noreferrer, etc.) |

#### Floating Button

| Property | Type | Description |
|----------|------|-------------|
| `Float` | `bool` | Enable floating button positioning mode - default: false |
| `FloatAbsolute` | `bool` | Use absolute positioning for floating button - default: false |

#### Button Type

| Property | Type | Description |
|----------|------|-------------|
| `ButtonType` | `BitButtonType` | HTML button type (Button, Submit, Reset) - default: Button |

#### Accessibility

| Property | Type | Description |
|----------|------|-------------|
| `AriaLabel` | `string` | Accessible label for screen readers |
| `AriaDescription` | `string` | Extended description for accessibility |
| `AriaHidden` | `bool?` | Hide from accessibility tree when true |
| `AllowDisabledFocus` | `bool` | Allow focus on disabled buttons for accessibility - default: false |

### Enumerations

#### BitVariant
Controls the visual style of the button:
- `Fill` (default) - Solid background with text
- `Outline` - Border only with transparent background
- `Text` - Text only without background or border

#### BitColor
Supports 16 semantic and neutral color variants:
- `Primary` (default)
- `Secondary`
- `Tertiary`
- `Info`
- `Success`
- `Warning`
- `SevereWarning`
- `Error`
- Background color variants
- Foreground color variants
- Border color variants

#### BitSize
Button dimension options:
- `Small` - Compact size for dense layouts
- `Medium` (default) - Standard size for most use cases
- `Large` - Prominent size for important actions

#### BitButtonType
HTML button type attribute:
- `Button` (default) - Standard button
- `Submit` - Form submission
- `Reset` - Form reset

### Events

#### OnClick
Triggered when the button is clicked.

```csharp
[Parameter]
public EventCallback<bool> OnClick { get; set; }
```

**Parameters:**
- `bool` - Current loading state of the button (true if loading, false otherwise)

### Code Examples

#### Basic Button
```html
<BitButton>Click Me</BitButton>
```

#### Button with Click Handler
```html
<BitButton OnClick="HandleClick">Submit</BitButton>

@code {
    private async Task HandleClick(bool isLoading)
    {
        // isLoading indicates if button is in loading state
        await ProcessData();
    }
}
```

#### Primary and Variant Buttons
```html
<!-- Primary Fill (default) -->
<BitButton Color="BitColor.Primary">Primary</BitButton>

<!-- Outline Variant -->
<BitButton Variant="BitVariant.Outline" Color="BitColor.Secondary">
    Outline Secondary
</BitButton>

<!-- Text Variant -->
<BitButton Variant="BitVariant.Text" Color="BitColor.Tertiary">
    Text Tertiary
</BitButton>
```

#### Semantic Color Buttons
```html
<BitButton Color="BitColor.Success">Success Action</BitButton>
<BitButton Color="BitColor.Warning">Warning Action</BitButton>
<BitButton Color="BitColor.Error">Delete</BitButton>
<BitButton Color="BitColor.Info">Information</BitButton>
```

#### Size Variants
```html
<BitButton Size="BitSize.Small">Small Button</BitButton>
<BitButton Size="BitSize.Medium">Medium Button</BitButton>
<BitButton Size="BitSize.Large">Large Button</BitButton>
```

#### Loading State with AutoLoading
```html
<BitButton AutoLoading="true" OnClick="PerformAsyncAction">
    Save Changes
</BitButton>

@code {
    private async Task PerformAsyncAction(bool isLoading)
    {
        // AutoLoading automatically manages loading state
        await Task.Delay(2000); // Simulate async work
    }
}
```

#### Manual Loading State
```html
<BitButton IsLoading="@isProcessing" LoadingLabel="Processing...">
    Process Data
</BitButton>

@code {
    private bool isProcessing = false;

    private async Task HandleClick(bool isLoading)
    {
        isProcessing = true;
        await Task.Delay(2000);
        isProcessing = false;
    }
}
```

#### Button with Icon
```html
<!-- Using icon name -->
<BitButton IconName="CheckMark">Confirm</BitButton>

<!-- Using icon URL -->
<BitButton IconUrl="https://example.com/icon.svg">Upload</BitButton>
```

#### Disabled and Full-Width
```html
<BitButton IsDisabled="true">Disabled Button</BitButton>

<BitButton FullWidth="true">Full Width Button</BitButton>
```

#### Form Submission
```html
<form>
    <input type="text" placeholder="Enter name" />
    <BitButton ButtonType="BitButtonType.Submit">Submit Form</BitButton>
    <BitButton ButtonType="BitButtonType.Reset">Reset</BitButton>
</form>
```

#### Navigation Button
```html
<!-- Navigate to URL -->
<BitButton Href="/dashboard" Target="_self">
    Go to Dashboard
</BitButton>

<!-- Open in new tab -->
<BitButton Href="https://example.com" Target="_blank" Rel="noopener noreferrer">
    External Link
</BitButton>
```

#### Floating Button
```html
<BitButton Float="true">+</BitButton>

<!-- With absolute positioning -->
<BitButton Float="true" FloatAbsolute="true" Style="bottom: 20px; right: 20px;">
    Action
</BitButton>
```

#### Custom Styling
```html
<BitButton
    Class="custom-button"
    Style="padding: 12px 24px; border-radius: 8px;">
    Custom Styled Button
</BitButton>

<style>
    .custom-button {
        font-weight: bold;
        text-transform: uppercase;
    }
</style>
```

#### Accessibility Example
```html
<BitButton
    AriaLabel="Save document changes"
    AriaDescription="Click to save all unsaved changes to the current document">
    Save
</BitButton>

<!-- Disable focus on disabled buttons for accessibility -->
<BitButton
    IsDisabled="true"
    AllowDisabledFocus="false">
    Unavailable
</BitButton>
```

#### Secondary Content with Template
```html
<BitButton>
    <PrimaryTemplate>
        Main Action
    </PrimaryTemplate>
    <SecondaryTemplate>
        <small>Subtitle</small>
    </SecondaryTemplate>
</BitButton>
```

### CSS Customization

The BitButton component exposes CSS classes and inline styles through:

- **`Class` Property**: Add custom CSS classes
- **`Style` Property**: Add inline CSS styles

Common customization patterns:

```html
<!-- Custom padding and border radius -->
<BitButton Style="padding: 8px 16px; border-radius: 4px;">
    Custom Spacing
</BitButton>

<!-- Custom font styling -->
<BitButton Class="bold-text uppercase">
    Styled Text
</BitButton>

<!-- Custom colors via CSS -->
<BitButton Class="gradient-button">
    Gradient Background
</BitButton>

@section Styles {
    <style>
        .gradient-button {
            background: linear-gradient(45deg, #667eea 0%, #764ba2 100%);
        }

        .bold-text {
            font-weight: 700;
        }

        .uppercase {
            text-transform: uppercase;
        }
    </style>
}
```

### Real-World Usage Patterns

#### Form with Validation
```html
<BitButton
    ButtonType="BitButtonType.Submit"
    IsDisabled="@(!isFormValid)"
    Color="BitColor.Primary">
    Submit
</BitButton>
```

#### Async Data Action
```html
<BitButton
    AutoLoading="true"
    OnClick="DeleteItem"
    Color="BitColor.Error">
    Delete Item
</BitButton>

@code {
    private async Task DeleteItem(bool isLoading)
    {
        await ApiClient.DeleteAsync("/api/items/1");
        await LoadItems();
    }
}
```

#### Call-to-Action Group
```html
<div class="button-group">
    <BitButton Variant="BitVariant.Outline">Cancel</BitButton>
    <BitButton Color="BitColor.Primary" FullWidth="true">
        Confirm Action
    </BitButton>
</div>

<style>
    .button-group {
        display: flex;
        gap: 8px;
    }
</style>
```

#### Loading Indicator with Progress
```html
<BitButton
    IsLoading="@isProcessing"
    LoadingLabel="@loadingProgress%"
    Reclickable="false">
    Process
</BitButton>

@code {
    private bool isProcessing = false;
    private int loadingProgress = 0;

    private async Task HandleProcess(bool isLoading)
    {
        isProcessing = true;

        for (int i = 0; i <= 100; i += 10)
        {
            loadingProgress = i;
            await Task.Delay(200);
        }

        isProcessing = false;
    }
}
```

### Accessibility Features

- **ARIA Labels**: Use `AriaLabel` and `AriaDescription` for screen reader users
- **Keyboard Navigation**: Fully keyboard accessible with tab navigation
- **Disabled State**: Properly indicated to assistive technologies
- **Focus Management**: `AllowDisabledFocus` option for advanced focus scenarios
- **Semantic HTML**: Generated as proper `<button>` or `<a>` elements based on context

### Best Practices

1. **Use semantic colors** for action significance (Error for destructive, Success for confirmations)
2. **Provide loading feedback** for long-running operations with `AutoLoading`
3. **Include descriptive text** or `AriaLabel` for icon-only buttons
4. **Set `FullWidth="true"`** in mobile-first responsive designs
5. **Use `Target="_blank"`** with `Rel="noopener noreferrer"` for external links
6. **Keep button labels concise** and action-oriented
7. **Disable buttons** during processing to prevent duplicate submissions

---

## ButtonGroup

### Component Description

The **BitButtonGroup** is a component designed to group related buttons together. It provides multiple ways to define buttons and supports various display modes including toggle functionality, vertical layout, and icon-only representations. This component is useful for organizing button collections with consistent styling and behavior.

### Key Features

- **Multiple Definition Methods**: Define buttons using BitButtonGroupItem class, custom generic classes, or BitButtonGroupOption child components
- **Toggle Mode**: Enable toggle functionality to track selected/active button states
- **Flexible Layout**: Support for horizontal and vertical button arrangements
- **Icon Support**: Display icons with text or icon-only mode
- **Variant Styling**: Support for Fill, Outline, and Text visual variants
- **Responsive Design**: Can expand to full container width with FullWidth option

### Component Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>` | `new List<TItem>()` | The collection of button items to display in the group |
| `Variant` | `BitVariant?` | `null` | Visual style variant: Fill, Outline, or Text |
| `Color` | `BitColor?` | `null` | Color theme applied to all buttons in the group |
| `Size` | `BitSize?` | `null` | Button size: Small, Medium, or Large |
| `Vertical` | `bool` | `false` | Renders buttons in a vertical stack instead of horizontal row |
| `Toggle` | `bool` | `false` | Enables toggle mode for buttons |
| `ToggleKey` | `string?` | `null` | Currently selected/toggled item key (two-way bindable) |
| `FullWidth` | `bool` | `false` | Expand buttons to fill 100% of container width |
| `IconOnly` | `bool` | `false` | Display only icons without text labels |
| `Classes` | `BitButtonGroupClassStyles?` | `null` | CSS class customization for component parts |
| `Styles` | `BitButtonGroupClassStyles?` | `null` | Inline style customization for component parts |

### Defining Button Items

#### Using BitButtonGroupItem

Create buttons using the `BitButtonGroupItem` class with the following properties:

```csharp
public class BitButtonGroupItem
{
    public string Key { get; set; }          // Unique identifier for the button
    public string Text { get; set; }         // Display text for the button
    public string Title { get; set; }        // Tooltip/title attribute
    public string IconName { get; set; }     // Icon identifier (e.g., "Add", "Delete")
    public EventCallback OnClick { get; set; } // Click event handler
    
    // Toggle mode properties
    public string OnText { get; set; }       // Text when toggled ON
    public string OffText { get; set; }      // Text when toggled OFF
    public string OnIconName { get; set; }   // Icon when toggled ON
    public string OffIconName { get; set; }  // Icon when toggled OFF
    public bool ReversedIcon { get; set; }   // Swap icon-text positioning
}
```

#### Using BitButtonGroupOption Component

Use child components for a more declarative syntax:

```razor
<BitButtonGroup>
    <BitButtonGroupOption Key="save" Text="Save" IconName="Save" />
    <BitButtonGroupOption Key="delete" Text="Delete" IconName="Delete" />
    <BitButtonGroupOption Key="edit" Text="Edit" IconName="Edit" />
</BitButtonGroup>
```

#### Using Custom Generic Classes

Define button items with custom properties that match your data model, providing flexibility for binding to existing data structures.

### Common Usage Examples

#### Basic Horizontal Button Group

```razor
<BitButtonGroup @bind-ToggleKey="selectedKey" Toggle="true">
    <BitButtonGroupOption Key="option1" Text="Option 1" />
    <BitButtonGroupOption Key="option2" Text="Option 2" />
    <BitButtonGroupOption Key="option3" Text="Option 3" />
</BitButtonGroup>

@code {
    private string selectedKey = "option1";
}
```

#### Vertical Button Group with Icons

```razor
<BitButtonGroup Vertical="true" Color="BitColor.Primary" Variant="BitVariant.Fill">
    <BitButtonGroupOption Key="add" Text="Add" IconName="Add" />
    <BitButtonGroupOption Key="edit" Text="Edit" IconName="Edit" />
    <BitButtonGroupOption Key="delete" Text="Delete" IconName="Delete" />
</BitButtonGroup>
```

#### Icon-Only Toggle Group

```razor
<BitButtonGroup IconOnly="true" Toggle="true" @bind-ToggleKey="activeFilter">
    <BitButtonGroupOption Key="list" IconName="List" Title="List View" />
    <BitButtonGroupOption Key="grid" IconName="GridViewSmall" Title="Grid View" />
    <BitButtonGroupOption Key="table" IconName="Table" Title="Table View" />
</BitButtonGroup>

@code {
    private string activeFilter = "list";
}
```

#### Full Width Button Group with Custom Items

```razor
<BitButtonGroup Items="actionItems" FullWidth="true" OnItemClick="HandleItemClick">
</BitButtonGroup>

@code {
    private List<BitButtonGroupItem> actionItems = new()
    {
        new BitButtonGroupItem { Key = "save", Text = "Save", IconName = "Save" },
        new BitButtonGroupItem { Key = "draft", Text = "Save as Draft", IconName = "SaveAs" },
        new BitButtonGroupItem { Key = "cancel", Text = "Cancel", IconName = "Cancel" }
    };

    private async Task HandleItemClick(BitButtonGroupItem item)
    {
        await ProcessAction(item.Key);
    }
}
```

#### Toggle Group with Dynamic Text

```razor
<BitButtonGroup Toggle="true" @bind-ToggleKey="toggleState" Color="BitColor.Success">
    <BitButtonGroupOption Key="on" 
                          OnText="Enabled" 
                          OffText="Enable"
                          OnIconName="CheckMark"
                          OffIconName="CircleEmpty" />
    <BitButtonGroupOption Key="off"
                          OnText="Disabled"
                          OffText="Disable"
                          OnIconName="BlockedSite"
                          OffIconName="CircleEmpty" />
</BitButtonGroup>

@code {
    private string toggleState = "on";
}
```

#### Styled Button Group with Custom CSS

```razor
<BitButtonGroup Items="items" 
                 Classes="customClasses"
                 Toggle="true"
                 @bind-ToggleKey="selectedKey">
</BitButtonGroup>

@code {
    private List<BitButtonGroupItem> items = new() { /* items */ };
    private string selectedKey = "";

    private BitButtonGroupClassStyles customClasses = new()
    {
        Root = "custom-button-group-root",
        Button = "custom-button-style",
        ToggledButton = "custom-active-button"
    };
}
```

### Events and Callbacks

#### OnItemClick Event

Fires when any button in the group is clicked:

```razor
<BitButtonGroup Items="items" OnItemClick="@((item) => HandleClick(item))">
</BitButtonGroup>

@code {
    private async Task HandleClick(BitButtonGroupItem item)
    {
        Console.WriteLine($"Button clicked: {item.Key}");
        // Handle the click event
    }
}
```

#### OnToggleChange Event

Triggered when the toggled item changes in toggle mode:

```razor
<BitButtonGroup Toggle="true" 
                 @bind-ToggleKey="selectedKey"
                 OnToggleChange="@((key) => HandleToggleChange(key))">
</BitButtonGroup>

@code {
    private string selectedKey = "";

    private async Task HandleToggleChange(string key)
    {
        Console.WriteLine($"Toggle changed to: {key}");
        // Handle the toggle change
    }
}
```

#### Individual Button Click Handler

```razor
<BitButtonGroup>
    <BitButtonGroupOption Key="action1" 
                          Text="Action 1"
                          OnClick="@(() => HandleAction1())" />
    <BitButtonGroupOption Key="action2"
                          Text="Action 2"
                          OnClick="@(() => HandleAction2())" />
</BitButtonGroup>

@code {
    private async Task HandleAction1()
    {
        // Process action 1
    }

    private async Task HandleAction2()
    {
        // Process action 2
    }
}
```

### CSS Customization

The **BitButtonGroupClassStyles** class provides granular control over component styling:

```csharp
public class BitButtonGroupClassStyles
{
    public string Root { get; set; }            // Container element styles
    public string Button { get; set; }          // Individual button element styles
    public string Icon { get; set; }            // Icon element styles
    public string Text { get; set; }            // Button text content styles
    public string ToggledButton { get; set; }   // Active/toggled button state styles
}
```

#### CSS Customization Example

```razor
<BitButtonGroup Items="items"
                 Classes="customStyles"
                 Color="BitColor.Primary">
</BitButtonGroup>

@code {
    private BitButtonGroupClassStyles customStyles = new()
    {
        Root = "btn-group-custom",
        Button = "btn-custom-style",
        Icon = "btn-icon-custom",
        Text = "btn-text-custom",
        ToggledButton = "btn-active-state"
    };
}
```

CSS classes to apply:

```css
.btn-group-custom {
    gap: 8px;
    padding: 12px;
    border-radius: 8px;
    background-color: #f5f5f5;
}

.btn-custom-style {
    min-width: 100px;
    border-radius: 4px;
    transition: all 0.3s ease;
}

.btn-icon-custom {
    margin-right: 8px;
    font-size: 18px;
}

.btn-text-custom {
    font-weight: 500;
    text-transform: uppercase;
}

.btn-active-state {
    background-color: #0078d4;
    color: white;
    box-shadow: 0 2px 8px rgba(0, 120, 212, 0.3);
}
```

### CSS Selectors for Custom Styling

The **BitButtonGroupNameSelectors** class provides CSS selectors for styling specific parts:

- **Root selector**: Targets the main container
- **Button selector**: Targets individual button elements
- **Icon selector**: Targets icon elements within buttons
- **Text selector**: Targets text content
- **ToggledButton selector**: Targets the active/selected button state

Use these selectors to apply custom CSS that overrides default styling.

---

This documentation provides a complete reference for implementing and customizing the BitButtonGroup component in your Blazor applications.

---

## MenuButton

### Overview
The **BitMenuButton** component displays a clickable button with a dropdown menu of options. It's a versatile multi-API component that supports multiple methods for providing and customizing menu items, with full styling control through CSS classes.

> A menu item that displays a word or phrase that the user can click to initiate an operation.

### Component Description
BitMenuButton provides a flexible dropdown menu interface that can be configured with simple text items or custom business objects. It supports three different APIs for defining menu items:
1. **BitMenuButtonItem** - Built-in simple item class
2. **Custom Generic Classes** - Use your own business objects with property mapping
3. **BitMenuButtonOption Components** - Declarative item definition via child components

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string?` | `null` | Header text displayed on the button |
| `IconName` | `string?` | `null` | Icon name to display in the header |
| `Items` | `IEnumerable<TItem>` | Empty list | Collection of menu items to display |
| `SelectedItem` | `TItem?` | `null` | Currently selected item in the menu |
| `DefaultSelectedItem` | `TItem?` | `null` | Initial selected item when component loads |
| `IsOpen` | `bool` | `false` | Controls whether the dropdown menu is visible |
| `Sticky` | `bool` | `false` | When true, the selected item text replaces the header text |
| `Split` | `bool` | `false` | When true, divides button into primary action and menu toggle sections |
| `Color` | `BitColor?` | `null` | Color variant (Primary, Secondary, Tertiary, etc.) |
| `Size` | `BitSize?` | `null` | Size variant (Small, Medium, or Large) |
| `Variant` | `BitVariant?` | `null` | Visual variant (Fill, Outline, or Text) |

### Events & Callbacks

- **OnChange**: `EventCallback<TItem>` - Triggered when a menu item is selected
- **OnClick**: `EventCallback<MouseEventArgs>` - Triggered when the header button is clicked
- **ItemTemplate**: `RenderFragment<TItem>` - Custom rendering for individual menu items
- **HeaderTemplate**: `RenderFragment` - Custom rendering for the button header

### Defining Menu Items

#### Method 1: Using BitMenuButtonItem (Simple Items)

```csharp
<BitMenuButton Items="items" />

@code {
    List<BitMenuButtonItem> items = new()
    {
        new() { Text = "Edit" },
        new() { Text = "Delete" },
        new() { Text = "Share" },
        new() { Text = "Export as PDF" }
    };
}
```

#### Method 2: Custom Objects with Property Mapping

Use your own business classes with the `NameSelectors` property to map them to the component:

```csharp
<BitMenuButton Items="actions" NameSelectors="actionSelectors" />

@code {
    public class UserAction
    {
        public string ActionName { get; set; }
        public string ActionKey { get; set; }
    }
    
    List<UserAction> actions = new()
    {
        new() { ActionName = "Change Password", ActionKey = "pwd" },
        new() { ActionName = "Privacy Settings", ActionKey = "privacy" }
    };
    
    BitMenuButtonNameSelectors<UserAction> actionSelectors = new()
    {
        Text = new(nameof(UserAction.ActionName))
    };
}
```

#### Method 3: Using BitMenuButtonOption Components

```csharp
<BitMenuButton Text="Options">
    <BitMenuButtonOption Text="New Document" />
    <BitMenuButtonOption Text="Open File" />
    <BitMenuButtonOption Text="Recent Files" />
</BitMenuButton>
```

### Common Usage Patterns

#### Basic Menu Button
```csharp
<BitMenuButton Text="Actions" Items="items" />

@code {
    List<BitMenuButtonItem> items = new()
    {
        new() { Text = "Edit" },
        new() { Text = "Delete" },
        new() { Text = "Archive" }
    };
}
```

#### Split Button (Two-Part Button)
Separates primary action from menu toggle:

```csharp
<BitMenuButton Split="true" 
               Text="Save" 
               OnClick="PrimaryAction"
               Items="saveOptions" />

@code {
    void PrimaryAction(MouseEventArgs args)
    {
        // Handle primary button click
    }
    
    List<BitMenuButtonItem> saveOptions = new()
    {
        new() { Text = "Save and Close" },
        new() { Text = "Save Draft" },
        new() { Text = "Save As Template" }
    };
}
```

#### Sticky Selection (Selected Item as Header)
When an item is selected, it replaces the header text:

```csharp
<BitMenuButton Sticky="true" 
               DefaultSelectedItem="selectedOption"
               Items="priorityOptions"
               OnChange="HandlePriorityChange" />

@code {
    BitMenuButtonItem selectedOption;
    
    List<BitMenuButtonItem> priorityOptions = new()
    {
        new() { Text = "Low" },
        new() { Text = "Medium" },
        new() { Text = "High" },
        new() { Text = "Critical" }
    };
    
    void HandlePriorityChange(BitMenuButtonItem item)
    {
        selectedOption = item;
    }
}
```

#### Styled Button with Color and Size
```csharp
<BitMenuButton Text="Actions"
               Color="BitColor.Primary"
               Size="BitSize.Large"
               Variant="BitVariant.Fill"
               Items="items" />
```

#### Custom Item Template
Customize how each menu item is rendered:

```csharp
<BitMenuButton Items="items">
    <ItemTemplate Context="item">
        <strong>@item.Text</strong>
    </ItemTemplate>
</BitMenuButton>
```

#### Complete Example with Event Handling
```csharp
<BitMenuButton Text="User Menu"
               Color="BitColor.Primary"
               Items="userActions"
               OnChange="HandleActionSelected"
               OnClick="HandleMenuClick">
    <ItemTemplate Context="action">
        <span class="menu-icon">@action.IconName</span>
        <span>@action.Text</span>
    </ItemTemplate>
</BitMenuButton>

@code {
    List<BitMenuButtonItem> userActions = new()
    {
        new() { Text = "Profile", IconName = "Contact" },
        new() { Text = "Settings", IconName = "Settings" },
        new() { Text = "Logout", IconName = "SignOut" }
    };
    
    void HandleActionSelected(BitMenuButtonItem item)
    {
        Console.WriteLine($"Selected: {item.Text}");
    }
    
    void HandleMenuClick(MouseEventArgs args)
    {
        Console.WriteLine("Menu button clicked");
    }
}
```

### Styling & CSS Customization

The component exposes extensive CSS class customization through the `BitMenuButtonClassStyles` property:

| CSS Class Property | Purpose |
|-------------------|---------|
| `Root` | Main container element |
| `Opened` | Applied when menu is open |
| `OperatorButton` | Primary button section |
| `ChevronDownButton` | Chevron/dropdown indicator button |
| `Callout` | Dropdown menu container |
| `ItemWrapper` | Container for each menu item |
| `ItemButton` | Individual menu item button |
| `ItemIcon` | Icon element within menu item |
| `ItemText` | Text element within menu item |
| `Overlay` | Overlay backdrop when menu is open |
| `Separator` | Visual separator between items |
| `Icon` | Header icon styling |
| `Text` | Header text styling |

**Example: Custom CSS Styling**

```csharp
<BitMenuButton Text="Options" Items="items" Class="custom-menu-button" />

<style>
    :deep(.custom-menu-button) {
        /* Customize root container */
    }
    
    :deep(.custom-menu-button .bit-mnu-btn--opened) {
        /* Styling when menu is open */
    }
    
    :deep(.custom-menu-button .bit-mnu-btn__item-button) {
        /* Style for individual menu items */
    }
</style>
```

### Key Features

- **Multi-API Support**: Choose between built-in items, custom objects, or component-based definition
- **Flexible Selection**: Optional sticky mode where selected items persist in header
- **Split Button Mode**: Separate primary action from menu toggle
- **Rich Customization**: Full control over styling via CSS classes
- **Template Support**: Custom rendering for headers and items
- **Event Handling**: Comprehensive click and change event support
- **Accessibility**: Proper ARIA labels and keyboard navigation support
- **Color & Size Variants**: Multiple styling options (Primary, Secondary, Small, Medium, Large, etc.)

### Best Practices

1. **Use Sticky for Status Selection**: Great for priority, status, or filter dropdowns where the selection should be visible
2. **Use Split for Dual Actions**: When you need both a primary action and secondary options
3. **Custom Objects with Selectors**: Leverage your domain models directly instead of mapping to BitMenuButtonItem
4. **Templates for Complex Items**: Use ItemTemplate when menu items need icons, descriptions, or formatted content
5. **Event Handling**: Use OnChange to react to user selections and update parent component state

This component is part of the Bit.BlazorUI framework and integrates seamlessly with other Bit components for consistent design and functionality across your Blazor applications.

---

## ToggleButton

ToggleButton is a Blazor UI component that provides a stateful button storing and displaying a toggle state (on/off or checked/unchecked). It allows users to switch between two states with visual feedback and full support for two-way binding, custom styling, and accessibility features.

### Component Overview

The ToggleButton component is ideal for:
- Feature toggles and switches
- Preference settings (on/off states)
- Boolean flag controls
- State-dependent UI interactions

### Core Parameters & Properties

#### State Management

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsChecked` | `bool` | `false` | Gets or sets the current toggle state (true = checked/on, false = unchecked/off) |
| `DefaultIsChecked` | `bool?` | `null` | Sets the initial checked state when the component first renders |

#### Display Content

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string?` | `null` | General label text displayed on the button |
| `OnText` | `string?` | `null` | Specific text to display when the toggle is checked (on state) |
| `OffText` | `string?` | `null` | Specific text to display when the toggle is unchecked (off state) |
| `IconName` | `string?` | `null` | Icon identifier to display alongside or instead of text |
| `OnIconName` | `string?` | `null` | Icon to display when the toggle is in the checked (on) state |
| `OffIconName` | `string?` | `null` | Icon to display when the toggle is in the unchecked (off) state |

#### Styling & Appearance

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Variant` | `BitVariant?` | `null` | Visual style variant: `Fill`, `Outline`, or `Text` |
| `Color` | `BitColor?` | `null` | Color scheme: `Primary`, `Secondary`, `Tertiary`, `Success`, `Warning`, `Info`, `Error`, etc. |
| `Size` | `BitSize?` | `null` | Button dimensions: `Small`, `Medium`, `Large` |
| `FixedColor` | `bool` | `false` | When true, preserves the color during hover and focus states (prevents color shift) |
| `IconOnly` | `bool` | `false` | When true, renders only the icon without text (useful for compact UI) |

#### Interaction & Accessibility

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsEnabled` | `bool` | `true` | Enables or disables the button; disabled buttons cannot be clicked or focused |
| `AllowDisabledFocus` | `bool` | `false` | Allows keyboard focus on disabled buttons (useful for accessibility) |
| `TabIndex` | `int?` | `null` | Controls tab order in keyboard navigation |
| `AriaLabel` | `string?` | `null` | Screen reader label for accessibility |
| `AriaDescription` | `string?` | `null` | Additional screen reader description for context |
| `AriaHidden` | `bool?` | `null` | Hides the component from accessibility tree when true |
| `Dir` | `string?` | `null` | Text direction: `ltr` (left-to-right) or `rtl` (right-to-left) |

#### Advanced Styling

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `BitToggleButtonClassStyles` | `BitToggleButtonClassStyles?` | `null` | Custom CSS classes for: Root, Checked, Icon, Text, and other sub-elements |
| `Class` | `string?` | `null` | Standard HTML class attribute |
| `Style` | `string?` | `null` | Standard HTML style attribute for inline CSS |
| `HtmlAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes to apply to the component |

### Events & Callbacks

#### OnChange

Fires when the `IsChecked` value changes. Enables custom logic in response to toggle state changes.

```csharp
// Callback signature
EventCallback<bool> OnChange

// Usage example
<BitToggleButton IsChecked="@isEnabled" OnChange="@HandleToggleChange" />

@code {
    private bool isEnabled = false;

    private async Task HandleToggleChange(bool newState)
    {
        isEnabled = newState;
        Console.WriteLine($"Toggle state changed to: {newState}");
        // Perform additional actions based on new state
        await SaveUserPreference(newState);
    }
}
```

#### OnClick

Fires when the button is clicked, receiving `MouseEventArgs` with mouse event details.

```csharp
// Callback signature
EventCallback<MouseEventArgs> OnClick

// Usage example
<BitToggleButton OnClick="@HandleButtonClick" />

@code {
    private async Task HandleButtonClick(MouseEventArgs args)
    {
        Console.WriteLine($"Button clicked at coordinates: {args.ClientX}, {args.ClientY}");
    }
}
```

### State Management Examples

#### Basic Toggle with Two-Way Binding

```razor
<BitToggleButton @bind-IsChecked="featureEnabled" />
<p>Feature is @(featureEnabled ? "Enabled" : "Disabled")</p>

@code {
    private bool featureEnabled = false;
}
```

#### Toggle with State-Specific Text

Display different text based on the toggle state:

```razor
<BitToggleButton
    @bind-IsChecked="notificationsOn"
    OnText="Notifications ON"
    OffText="Notifications OFF" />

@code {
    private bool notificationsOn = true;
}
```

#### Toggle with State-Specific Icons

Show different icons depending on the toggle state:

```razor
<BitToggleButton
    @bind-IsChecked="darkModeEnabled"
    OnIconName="Weather.Moon24Filled"
    OffIconName="Weather.Sun24Filled"
    OnText="Dark Mode"
    OffText="Light Mode"
    Color="BitColor.Primary" />

@code {
    private bool darkModeEnabled = false;
}
```

#### Icon-Only Toggle Button

Compact button displaying only an icon:

```razor
<BitToggleButton
    @bind-IsChecked="soundEnabled"
    OnIconName="VolumeHigh24Filled"
    OffIconName="VolumeMute24Filled"
    IconOnly="true"
    AriaLabel="Toggle sound" />

@code {
    private bool soundEnabled = true;
}
```

### Styling & Appearance Examples

#### Variant & Color Combinations

```razor
<!-- Fill variant with primary color -->
<BitToggleButton
    @bind-IsChecked="state1"
    Text="Primary Fill"
    Variant="BitVariant.Fill"
    Color="BitColor.Primary" />

<!-- Outline variant with secondary color -->
<BitToggleButton
    @bind-IsChecked="state2"
    Text="Secondary Outline"
    Variant="BitVariant.Outline"
    Color="BitColor.Secondary" />

<!-- Text variant with success color -->
<BitToggleButton
    @bind-IsChecked="state3"
    Text="Success Text"
    Variant="BitVariant.Text"
    Color="BitColor.Success" />

@code {
    private bool state1 = false;
    private bool state2 = true;
    private bool state3 = false;
}
```

#### Size Variations

```razor
<BitToggleButton @bind-IsChecked="small" Text="Small" Size="BitSize.Small" />
<BitToggleButton @bind-IsChecked="medium" Text="Medium" Size="BitSize.Medium" />
<BitToggleButton @bind-IsChecked="large" Text="Large" Size="BitSize.Large" />

@code {
    private bool small = false;
    private bool medium = false;
    private bool large = false;
}
```

#### Fixed Color (No Hover Shift)

```razor
<!-- Color stays consistent during hover/focus -->
<BitToggleButton
    @bind-IsChecked="alwaysBlue"
    Text="Fixed Color"
    Color="BitColor.Primary"
    FixedColor="true" />

@code {
    private bool alwaysBlue = false;
}
```

### Real-World Usage Examples

#### Feature Flag Toggle

```razor
<div class="feature-control">
    <h3>Feature Flags</h3>

    <BitToggleButton
        @bind-IsChecked="betaFeatures"
        OnText="Beta Features: ON"
        OffText="Beta Features: OFF"
        OnChange="@UpdateFeatureFlag"
        Color="BitColor.Warning" />

    @if (betaFeatures)
    {
        <div class="beta-warning">
            <strong>Note:</strong> Beta features are experimental and may change.
        </div>
    }
</div>

@code {
    private bool betaFeatures = false;

    private async Task UpdateFeatureFlag(bool enabled)
    {
        betaFeatures = enabled;
        await SaveSettingToDatabase("betaFeatures", enabled);
        StateHasChanged();
    }

    private async Task SaveSettingToDatabase(string key, bool value)
    {
        // Simulated database save
        Console.WriteLine($"Saved {key} = {value}");
    }
}
```

#### User Preference Panel

```razor
<div class="preferences-panel">
    <h2>Notification Preferences</h2>

    <div class="preference-item">
        <label>Email Notifications</label>
        <BitToggleButton
            @bind-IsChecked="emailNotifications"
            OnChange="@SavePreferences" />
    </div>

    <div class="preference-item">
        <label>Push Notifications</label>
        <BitToggleButton
            @bind-IsChecked="pushNotifications"
            OnChange="@SavePreferences" />
    </div>

    <div class="preference-item">
        <label>SMS Alerts</label>
        <BitToggleButton
            @bind-IsChecked="smsAlerts"
            OnChange="@SavePreferences" />
    </div>

    @if (saveStatus == "saved")
    {
        <p style="color: green;">Preferences saved successfully</p>
    }
</div>

@code {
    private bool emailNotifications = true;
    private bool pushNotifications = true;
    private bool smsAlerts = false;
    private string saveStatus = "";

    private async Task SavePreferences(bool value)
    {
        // Save individual preference or collect all and save together
        await Task.Delay(500); // Simulate API call
        saveStatus = "saved";
    }
}
```

#### Theme Switcher

```razor
<header class="app-header">
    <h1>My Application</h1>

    <BitToggleButton
        @bind-IsChecked="isDarkMode"
        OnIconName="Weather.Moon24Filled"
        OffIconName="Weather.Sun24Filled"
        OnText="Dark"
        OffText="Light"
        IconOnly="true"
        OnChange="@ApplyTheme"
        AriaLabel="Toggle dark/light theme" />
</header>

@code {
    private bool isDarkMode = false;

    private async Task ApplyTheme(bool darkMode)
    {
        isDarkMode = darkMode;
        var htmlElement = (IJSObjectReference)await JS.InvokeAsync<object>("document.documentElement");
        if (darkMode)
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.add('dark-theme')");
        }
        else
        {
            await JS.InvokeVoidAsync("eval", "document.body.classList.remove('dark-theme')");
        }
    }
}
```

#### Disabled State

```razor
<div class="toggle-disabled-example">
    <h3>Disabled Toggle Examples</h3>

    <!-- Fully disabled, cannot interact -->
    <BitToggleButton
        IsChecked="true"
        Text="Disabled (Checked)"
        IsEnabled="false" />

    <!-- Disabled but keyboard focusable -->
    <BitToggleButton
        IsChecked="false"
        Text="Disabled (Focusable)"
        IsEnabled="false"
        AllowDisabledFocus="true" />

    <!-- Enable/disable toggle with checkbox -->
    <label>
        <input type="checkbox" @onchange="@((ChangeEventArgs args) => canToggleFeature = (bool)args.Value!)" />
        Allow feature toggle
    </label>

    <BitToggleButton
        @bind-IsChecked="featureEnabled"
        Text="Feature Toggle"
        IsEnabled="@canToggleFeature" />
</div>

@code {
    private bool featureEnabled = true;
    private bool canToggleFeature = true;
}
```

### CSS Customization Options

The `BitToggleButtonClassStyles` property allows targeted CSS customization for different elements:

```csharp
// Available customization targets:
// - Root: Main button container
// - Checked: Styling when IsChecked = true
// - Icon: Icon element styling
// - Text: Text content styling
// - Other state-specific classes

// Usage pattern:
new BitToggleButtonClassStyles()
{
    Root = "custom-toggle-root",
    Checked = "custom-toggle-checked",
    Icon = "custom-toggle-icon",
    Text = "custom-toggle-text"
}
```

Example with custom CSS:

```razor
<BitToggleButton
    @bind-IsChecked="customStyled"
    Text="Custom Styled"
    Class="my-custom-toggle"
    Style="border-radius: 20px; padding: 12px 24px;" />

@code {
    private bool customStyled = false;
}
```

### Accessibility Features

The ToggleButton component provides comprehensive accessibility support:

```razor
<!-- With accessibility labels -->
<BitToggleButton
    @bind-IsChecked="screenReaderExample"
    Text="Enable Accessibility"
    AriaLabel="Toggle accessibility mode"
    AriaDescription="Enable advanced accessibility features for screen readers and keyboard navigation" />

<!-- RTL Support -->
<BitToggleButton
    @bind-IsChecked="rtlExample"
    Text="Right-to-Left"
    Dir="rtl" />

<!-- Tab Order Control -->
<BitToggleButton
    @bind-IsChecked="tabControlExample"
    Text="Priority Focus"
    TabIndex="0" />

@code {
    private bool screenReaderExample = false;
    private bool rtlExample = false;
    private bool tabControlExample = false;
}
```

### Summary

The ToggleButton component is a versatile, accessible control for binary state management in Blazor applications. Key features include:

- **Two-way binding** via `@bind-IsChecked`
- **State-specific content** (OnText/OffText, OnIconName/OffIconName)
- **Rich styling options** (Variant, Color, Size, FixedColor)
- **Complete accessibility support** (AriaLabel, AriaDescription, keyboard navigation)
- **Event callbacks** (OnChange, OnClick) for custom logic
- **Disabled state** with optional keyboard focus
- **CSS customization** for fine-grained styling control

This makes ToggleButton ideal for feature flags, preferences, theme switching, and any boolean state management in your Blazor UI.

---

# Inputs

## Calendar

The **BitCalendar** component is a date/time picker control that enables users to select and view single dates or date ranges. It features three separate views (month, year, and decade) with optional time picker functionality, flexible date constraints, and comprehensive customization options.

### Component Overview

BitCalendar is a versatile calendar control that integrates seamlessly into Blazor forms and applications. It supports:
- Single date or date range selection
- Month, year, and decade navigation views
- Optional integrated time picker
- Culture-aware formatting and localization
- Timezone support
- Comprehensive CSS customization
- Custom cell templates for specialized rendering

### Core Parameters

#### Date & Selection Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `TValue?` | `null` | Two-way bindable selected date value |
| `StartingValue` | `DateTimeOffset?` | `null` | Initial display date when no selection exists |
| `MinDate` | `DateTimeOffset?` | `null` | Minimum selectable date (constrains backward navigation) |
| `MaxDate` | `DateTimeOffset?` | `null` | Maximum selectable date (constrains forward navigation) |
| `ShowTimePicker` | `bool` | `false` | Enables integrated time selection interface |
| `TimeFormat` | `BitTimeFormat` | `TwentyFourHours` | Time display format: `TwentyFourHours` (0) or `TwelveHours` (1) |
| `HourStep` | `int` | `1` | Increment/decrement interval for hour selection |
| `MinuteStep` | `int` | `1` | Increment/decrement interval for minute selection |

#### Display & Navigation Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowWeekNumbers` | `bool` | `false` | Displays ISO week numbering (1-53) in calendar grid |
| `ShowMonthPicker` | `bool` | `true` | Shows month selection dropdown/picker |
| `ShowMonthPickerAsOverlay` | `bool` | `false` | Renders month picker as floating overlay instead of dropdown |
| `ShowGoToToday` | `bool` | `true` | Displays "Go to Today" navigation button |
| `ShowGoToNow` | `bool` | `true` | Displays "Go to Now" navigation button (time picker mode) |
| `HighlightCurrentMonth` | `bool` | `false` | Visually highlights current month in month view |
| `HighlightSelectedMonth` | `bool` | `false` | Visually highlights selected month in month view |
| `Dir` | `BitDir` | `Ltr` | Text direction: `Ltr` (left-to-right), `Rtl` (right-to-left), or `Auto` |

#### Formatting & Localization Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Culture` | `CultureInfo` | `CurrentUICulture` | Culture used for date/time formatting and localization |
| `DateFormat` | `string?` | `null` | Custom date display format (e.g., "MM/dd/yyyy" or "dd-MMM-yyyy") |
| `TimeZone` | `TimeZoneInfo?` | `null` | Timezone for date/time interpretation and display |

#### Validation & Input Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Required` | `bool` | `false` | Makes date selection mandatory in forms |
| `NoValidate` | `bool` | `false` | Disables validation when `true` |
| `InvalidErrorMessage` | `string?` | `null` | Custom error message for validation failures |
| `ReadOnly` | `bool` | `false` | Prevents manual time editing in text input |
| `DisplayName` | `string?` | `null` | Accessible field label for screen readers and validation |

#### Styling & Customization Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Classes` | `BitCalendarClassStyles` | `new()` | CSS class customization for 50+ component elements |
| `Styles` | `BitCalendarStyleStyles` | `new()` | Inline style customization for component parts |
| `DayCellTemplate` | `RenderFragment?` | `null` | Custom template for day cells in month view |
| `MonthCellTemplate` | `RenderFragment?` | `null` | Custom template for cells in month picker view |
| `YearCellTemplate` | `RenderFragment?` | `null` | Custom template for cells in year/decade view |

### Events & Callbacks

#### Date Selection Events

| Event | Type | Description |
|-------|------|-------------|
| `OnSelectDate` | `EventCallback<DateTimeOffset?>` | Fires when user selects a date in the calendar |
| `OnChange` | `EventCallback<TValue?>` | Triggered when the input value changes (after validation) |

### Public Methods

- **FocusAsync()** - Sets focus to the calendar input element
- **FocusAsync(bool preventScroll)** - Sets focus with optional scroll prevention

### Code Examples

#### Basic Calendar Usage

```csharp
<BitCalendar @bind-Value="selectedDate" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### Calendar with Date Range Constraints

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    MinDate="@DateTime.Now.AddDays(-5)"
    MaxDate="@DateTime.Now.AddDays(5)"
    StartingValue="@DateTime.Now" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### Calendar with Time Picker

```csharp
<BitCalendar 
    @bind-Value="selectedDateTime"
    ShowTimePicker="true"
    TimeFormat="BitTimeFormat.TwentyFourHours"
    HourStep="2"
    MinuteStep="15" />

@code {
    private DateTimeOffset? selectedDateTime;
}
```

#### 12-Hour Time Format

```csharp
<BitCalendar 
    @bind-Value="selectedDateTime"
    ShowTimePicker="true"
    TimeFormat="BitTimeFormat.TwelveHours" />

@code {
    private DateTimeOffset? selectedDateTime;
}
```

#### Custom Date Formatting with Culture Support

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    Culture="@CultureInfo.GetCultureInfo("de-DE")"
    DateFormat="dd.MM.yyyy" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### Calendar with Week Numbers and Month Highlight

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    ShowWeekNumbers="true"
    HighlightCurrentMonth="true"
    HighlightSelectedMonth="true" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### Form Validation Example

```csharp
<EditForm Model="bookingData" OnValidSubmit="HandleValidSubmit">
    <DataAnnotationsValidator />
    
    <BitCalendar 
        @bind-Value="bookingData.EventDate"
        Required="true"
        InvalidErrorMessage="Please select an event date"
        MinDate="@DateTime.Now"
        DisplayName="Event Date" />
    
    <ValidationMessage For="@(() => bookingData.EventDate)" />
    
    <button type="submit">Book Event</button>
</EditForm>

@code {
    private BookingModel bookingData = new();
    
    private void HandleValidSubmit()
    {
        // Process booking with selected date
    }
    
    class BookingModel
    {
        public DateTimeOffset? EventDate { get; set; }
    }
}
```

#### Date Selection with Event Handling

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    OnSelectDate="HandleDateSelected" />

<p>Selected: @selectedDate?.ToString("MMMM dd, yyyy")</p>

@code {
    private DateTimeOffset? selectedDate;
    
    private async Task HandleDateSelected(DateTimeOffset? date)
    {
        selectedDate = date;
        // Perform additional logic when date is selected
        await LoadContentForDate(date);
    }
    
    private async Task LoadContentForDate(DateTimeOffset? date)
    {
        // Load data relevant to selected date
    }
}
```

#### Timezone-Aware Calendar

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    TimeZone="TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")"
    ShowTimePicker="true"
    DateFormat="MM/dd/yyyy HH:mm" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### Custom Calendar Cell Template

```csharp
<BitCalendar @bind-Value="selectedDate">
    <DayCellTemplate>
        <div class="custom-day-cell">
            <!-- Custom day cell rendering -->
        </div>
    </DayCellTemplate>
    <MonthCellTemplate>
        <div class="custom-month-cell">
            <!-- Custom month cell rendering -->
        </div>
    </MonthCellTemplate>
    <YearCellTemplate>
        <div class="custom-year-cell">
            <!-- Custom year cell rendering -->
        </div>
    </YearCellTemplate>
</BitCalendar>

@code {
    private DateTimeOffset? selectedDate;
}
```

### CSS Customization

BitCalendar provides granular control over styling through the `Classes` and `Styles` parameters. The component exposes 50+ customizable CSS classes and styles for different parts:

#### Available Customization Points

- **Root** - Main calendar container
- **Container** - Calendar wrapper
- **DayPickerWrapper** - Month view day grid
- **MonthButton** - Month navigation button
- **TimePickerContainer** - Time selection area
- **InputWrapper** - Input field container
- **DayButton** - Individual day cell
- **MonthCell** - Month picker cell
- **YearCell** - Year/decade view cell

#### Custom Styling Example

```csharp
<BitCalendar 
    @bind-Value="selectedDate"
    Classes="@customClasses"
    Styles="@customStyles" />

@code {
    private DateTimeOffset? selectedDate;
    
    private BitCalendarClassStyles customClasses = new()
    {
        Root = "custom-calendar-root",
        DayButton = "custom-day-button",
        // ... additional class customizations
    };
    
    private BitCalendarStyleStyles customStyles = new()
    {
        // Custom inline styles for component parts
    };
}
```

### Key Features

- **Multi-View Navigation**: Seamlessly switch between day, month, year, and decade views
- **Flexible Date Constraints**: Enforce min/max date ranges for business logic
- **Integrated Time Picker**: Optional time selection with configurable step intervals
- **Localization Ready**: Culture-aware formatting with timezone support
- **Form Integration**: Full validation support within EditForm components
- **Accessible**: ARIA-compliant with keyboard navigation support
- **RTL Support**: Built-in right-to-left directionality for international applications
- **Templatable**: Custom rendering for specialized calendar presentations
- **Month Picker Overlay**: Optional floating month picker for compact layouts

### Practical Real-World Scenarios

1. **Meeting Scheduling**: Use min/max dates to prevent booking in the past or beyond available slots
2. **Form Validation**: Require date selection with custom validation messages for mandatory fields
3. **Timezone-Aware Bookings**: Display dates in user's preferred timezone while storing UTC internally
4. **Event Planning**: Combine date and time selection with validation for event booking systems
5. **Report Date Ranges**: Use as part of filtering controls in analytics dashboards
6. **Appointment Scheduling**: Integrate with time-slot availability checking using custom cell templates

---

## Checkbox

The **BitCheckbox** component is a flexible binary choice input that allows users to select between checked and unchecked states. It supports advanced features like indeterminate states for hierarchical selections, multiple size and color variants, and extensive customization options through templates and styling.

### Component Description

BitCheckbox renders as a standard checkbox control with visual states for unchecked (☐), checked (☑), disabled, and indeterminate modes. It integrates seamlessly with Blazor's two-way data binding and validation systems, making it suitable for forms, settings panels, and hierarchical selection scenarios.

### Properties & Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Value` | `TValue?` | Two-way bindable checkbox state (controlled component mode) |
| `DefaultValue` | `bool?` | Initial state for uncontrolled component mode |
| `Label` | `string?` | Descriptive text displayed next to the checkbox |
| `Indeterminate` | `bool` | Visual third state for partial selection; independent of `Value` |
| `DefaultIndeterminate` | `bool?` | Initial indeterminate state for uncontrolled components |
| `Reversed` | `bool` | Positions label after checkbox instead of before |
| `Size` | `BitSize?` | Component size variant: `Small`, `Medium`, or `Large` |
| `Color` | `BitColor?` | Color theme with 16+ options: `Primary`, `Secondary`, `Success`, `Error`, `Warning`, `Info`, etc. |
| `CheckIconName` | `string` | Custom icon name for the check mark (replaces default) |
| `LabelTemplate` | `RenderFragment?` | Custom template for label rendering |
| `ChildContent` | `RenderFragment?` | Full content customization fragment |
| `Dir` | `BitDir` | Text direction: `Ltr` (default) or `Rtl` for right-to-left languages |
| `AriaLabel` | `string?` | Screen reader label for accessibility |
| `AriaDescription` | `string?` | Screen reader description |
| `AriaLabelledby` | `string?` | ID of element describing the checkbox |
| `Classes` | `BitCheckboxClassStyles?` | CSS class customization for elements |
| `Styles` | `BitCheckboxClassStyles?` | Inline style customization for elements |

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<TValue?>` | Fires when the checked state changes |
| `OnClick` | `EventCallback<MouseEventArgs>` | Fires on user click interaction |

### Indeterminate State Handling

The indeterminate state provides a visual intermediate state independent from the actual `Value` property. This is particularly useful for parent-child checkbox hierarchies where:

- **Parent checkbox** displays indeterminate when some (but not all) child checkboxes are checked
- **Child checkboxes** have standard checked/unchecked values
- The visual appearance differs from checked/unchecked, providing clear user feedback

Set `Indeterminate="true"` to enable this visual state without affecting the underlying boolean value.

### Usage Examples

#### Basic Binding

```razor
<!-- Controlled component with two-way binding -->
<BitCheckbox @bind-Value="isAgreeToTerms" 
             Label="I agree to the terms of service" />

<!-- Uncontrolled component with default value -->
<BitCheckbox DefaultValue="true" 
             Label="Enable notifications" />
```

#### Indeterminate State (Parent-Child Selection)

```razor
@if (hasChildren)
{
    <BitCheckbox @bind-Value="parentChecked" 
                 @bind-Indeterminate="parentIndeterminate"
                 Label="Select all items" />
}

@foreach (var item in items)
{
    <BitCheckbox @bind-Value="item.IsSelected" 
                 Label="@item.Name"
                 Style="margin-left: 20px;" />
}

@code {
    bool parentChecked;
    bool parentIndeterminate;
    List<ItemModel> items = new();

    protected override void OnParametersSet()
    {
        int checkedCount = items.Count(i => i.IsSelected);
        parentChecked = checkedCount == items.Count;
        parentIndeterminate = checkedCount > 0 && checkedCount < items.Count;
    }

    void SelectAll(bool value)
    {
        foreach (var item in items) { item.IsSelected = value; }
        parentIndeterminate = false;
    }
}
```

#### Size & Color Variants

```razor
<!-- Different sizes -->
<BitCheckbox Label="Small" Size="BitSize.Small" />
<BitCheckbox Label="Medium" Size="BitSize.Medium" />
<BitCheckbox Label="Large" Size="BitSize.Large" />

<!-- Color variants -->
<BitCheckbox Label="Primary" Color="BitColor.Primary" @bind-Value="value1" />
<BitCheckbox Label="Success" Color="BitColor.Success" @bind-Value="value2" />
<BitCheckbox Label="Error" Color="BitColor.Error" @bind-Value="value3" />
<BitCheckbox Label="Warning" Color="BitColor.Warning" @bind-Value="value4" />
```

#### Custom Labels & Templates

```razor
<!-- Custom label with HTML -->
<BitCheckbox @bind-Value="acceptMarketing">
    <LabelTemplate>
        I accept marketing emails from <strong>@companyName</strong>
    </LabelTemplate>
</BitCheckbox>

<!-- Label positioned after checkbox -->
<BitCheckbox @bind-Value="isEnabled" 
             Label="Feature enabled" 
             Reversed="true" />

<!-- Full content customization -->
<BitCheckbox @bind-Value="premium">
    <ChildContent>
        <div class="checkbox-container">
            <strong>Premium Membership</strong>
            <p class="description">Unlock exclusive features and priority support</p>
        </div>
    </ChildContent>
</BitCheckbox>
```

#### Event Handling

```razor
<BitCheckbox @bind-Value="isChecked" 
             OnChange="HandleValueChanged"
             OnClick="HandleClick"
             Label="Subscribe to updates" />

@code {
    bool isChecked;

    private async Task HandleValueChanged(bool? newValue)
    {
        Console.WriteLine($"Checkbox value changed to: {newValue}");
        // Perform validation or side effects
        await SaveUserPreference(newValue ?? false);
    }

    private async Task HandleClick(MouseEventArgs args)
    {
        Console.WriteLine($"Checkbox clicked at ({args.ClientX}, {args.ClientY})");
    }

    private async Task SaveUserPreference(bool value)
    {
        // API call or service interaction
    }
}
```

#### CSS Customization

```razor
<!-- Custom styling via Classes -->
<BitCheckbox @bind-Value="isCustom" 
             Label="Custom styled"
             Classes="new BitCheckboxClassStyles
             {
                 Root = \"custom-checkbox-root\",
                 Container = \"custom-container\",
                 Checked = \"custom-checked\",
                 Box = \"custom-box\",
                 Icon = \"custom-icon\",
                 Label = \"custom-label\"
             }" />

<!-- Inline styles -->
<BitCheckbox @bind-Value="isStyled" 
             Label="Styled checkbox"
             Styles="new BitCheckboxClassStyles
             {
                 Root = \"margin: 12px; padding: 8px;\",
                 Label = \"font-weight: 600; color: #2c3e50;\"
             }" />
```

#### Accessibility

```razor
<BitCheckbox @bind-Value="acceptTerms"
             AriaLabel="Accept terms and conditions"
             AriaDescription="Check this box to agree to our terms of service before proceeding"
             Label="I accept the terms" />

<BitCheckbox @bind-Value="accessibility"
             AriaLabelledby="terms-heading"
             Label="Enable accessibility features" />

<h2 id="terms-heading">Terms of Service</h2>
```

#### Right-to-Left (RTL) Support

```razor
<BitCheckbox @bind-Value="isRtl"
             Dir="BitDir.Rtl"
             Label="موافق" />
```

### CSS Customization Options

The `BitCheckboxClassStyles` object allows granular control over component elements:

| Element | Purpose |
|---------|---------|
| `Root` | Outer container wrapper |
| `Container` | Inner checkbox container |
| `Checked` | Applied when checkbox is checked |
| `Box` | Visual checkbox box element |
| `Icon` | Check mark icon inside box |
| `Label` | Text label element |

### Validation Integration

BitCheckbox inherits from `BitInputBase` and integrates with Blazor's data annotation validation:

```razor
<EditForm Model="formModel" OnValidSubmit="HandleSubmit">
    <DataAnnotationsValidator />
    
    <BitCheckbox @bind-Value="formModel.AcceptTerms"
                 Label="Accept terms and conditions"
                 Class="mb-3" />
    <ValidationMessage For="() => formModel.AcceptTerms" />
    
    <button type="submit">Submit</button>
</EditForm>

@code {
    [Required(ErrorMessage = "You must accept the terms")]
    public bool AcceptTerms { get; set; }
}
```

### Best Practices

- **Use clear labels**: Always provide descriptive text for checkboxes to improve usability
- **Validate required checkboxes**: Use data annotations for mandatory acceptance checkboxes (terms, privacy)
- **Indeterminate for hierarchy**: Use indeterminate state in master-detail scenarios for clearer UX
- **Accessibility**: Always include `AriaLabel` or `AriaDescription` for screen readers
- **Color semantics**: Use `BitColor.Error` for destructive actions, `BitColor.Success` for confirmations
- **Icon customization**: Replace default check icons only when necessary for brand consistency

Sources:
- [Bit.BlazorUI Checkbox Component](https://blazorui.bitplatform.dev/components/checkbox)

---

## ChoiceGroup

The **BitChoiceGroup** component enables users to select a single option from two or more choices. It's a flexible, multi-API component that can accept items through multiple methods: BitChoiceGroupItem class, custom generic classes, or BitChoiceGroupOption components.

### Component Description

The ChoiceGroup presents a set of options where users can select exactly one choice at a time. It's ideal for preference selection, filtering, or settings where mutually exclusive options are needed. The component supports both simple text labels and rich content with icons or images.

### Parameters & Properties

| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `Items` | `IEnumerable<TItem>` | `new List<TItem>()` | Populates the choice list with data items |
| `Horizontal` | `bool` | `false` | Arranges items horizontally instead of vertically |
| `Inline` | `bool` | `false` | Renders icons/images with items in a single line layout |
| `Reversed` | `bool` | `false` | Repositions label and radio button (swaps their positions) |
| `NoCircle` | `bool` | `false` | Removes the circular radio button indicator |
| `Label` | `string?` | `null` | Label text displayed above or with the ChoiceGroup |
| `DefaultValue` | `string?` | `null` | Pre-selected option value on component initialization |
| `Value` | `TValue?` | `null` | Current selected value (for data binding) |
| `Color` | `BitColor?` | `null` | Visual color variant (Primary, Secondary, Success, Warning, Danger, Info, etc.) |
| `Size` | `BitSize?` | `null` | Size variant: Small, Medium, or Large |

### Choice Definition Methods

The ChoiceGroup supports three approaches for defining options:

#### Method 1: BitChoiceGroupOption Component (Child Elements)

Define choices directly as child components:

```razor
<BitChoiceGroup @bind-Value="selectedOption" Label="Select Your Option">
    <BitChoiceGroupOption Text="Option 1" Value="opt1" />
    <BitChoiceGroupOption Text="Option 2" Value="opt2" IconName="Icon.Home" />
    <BitChoiceGroupOption Text="Option 3" Value="opt3" ImageSrc="/images/icon3.png" />
</BitChoiceGroup>

@code {
    private string selectedOption = "opt1";
}
```

#### Method 2: BitChoiceGroupItem Class (Data Source)

Use a data-driven approach with BitChoiceGroupItem:

```razor
<BitChoiceGroup Items="choiceItems"
                @bind-Value="selectedValue"
                Label="Select a Framework">
</BitChoiceGroup>

@code {
    private List<BitChoiceGroupItem> choiceItems = new()
    {
        new BitChoiceGroupItem { Text = "Blazor", Value = "blazor", IconName = "Icon.Code" },
        new BitChoiceGroupItem { Text = "React", Value = "react", IconName = "Icon.Code" },
        new BitChoiceGroupItem { Text = "Vue", Value = "vue", IconName = "Icon.Code" }
    };

    private string selectedValue = "blazor";
}
```

#### Method 3: Custom Generic Class (Flexible Data Binding)

Bind to custom objects with property mapping:

```razor
<BitChoiceGroup Items="roles"
                NameSelectors="roleNameSelectors"
                @bind-Value="selectedRoleId"
                Label="Select User Role">
</BitChoiceGroup>

@code {
    private class UserRole
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    private List<UserRole> roles = new()
    {
        new() { Id = 1, Title = "Administrator", Description = "Full system access" },
        new() { Id = 2, Title = "Editor", Description = "Content editing only" },
        new() { Id = 3, Title = "Viewer", Description = "Read-only access" }
    };

    private int selectedRoleId = 1;

    private BitChoiceGroupItem.NameSelectors<UserRole> roleNameSelectors =
        new()
        {
            Text = item => item.Title,
            Value = item => item.Id.ToString()
        };
}
```

### Choice Option Properties

Each choice option (whether defined as BitChoiceGroupOption, BitChoiceGroupItem, or custom class) supports:

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | Display text for the option |
| `Value` | `string` | Unique value identifier for the option |
| `IconName` | `string?` | Icon name (e.g., "Icon.Home") to display with option |
| `ImageSrc` | `string?` | URL to image displayed for this option |
| `SelectedImageSrc` | `string?` | Alternative image shown when option is selected |
| `ImageAlt` | `string?` | Alt text for accessibility |
| `IsEnabled` | `bool` | Whether the option is enabled or disabled |
| `AriaLabel` | `string?` | ARIA label for accessibility |
| `Class` | `string?` | Custom CSS classes for styling |
| `Style` | `string?` | Inline styles |
| `Id` | `string?` | HTML element ID |
| `Prefix` | `string?` | Text or content displayed before the option |
| `ImageSize` | `string?` | Size specification for images (e.g., "64px") |
| `Template` | `RenderFragment?` | Custom template for rendering the option |

### Events & Callbacks

#### OnChange Event

Triggered when the user changes the selected option:

```razor
<BitChoiceGroup Items="options"
                @bind-Value="selectedValue"
                OnChange="HandleOptionChanged"
                Label="Select an Environment">
</BitChoiceGroup>

@code {
    private List<BitChoiceGroupItem> options = new()
    {
        new() { Text = "Development", Value = "dev" },
        new() { Text = "Staging", Value = "staging" },
        new() { Text = "Production", Value = "prod" }
    };

    private string selectedValue = "dev";

    private async Task HandleOptionChanged(string newValue)
    {
        selectedValue = newValue;
        // Perform async operations or validations
        await SavePreference(newValue);
    }
}
```

#### OnClick Event

Called when an option is clicked:

```razor
<BitChoiceGroup Items="choices"
                OnClick="HandleClick"
                Label="Select Notification Preference">
</BitChoiceGroup>

@code {
    private List<BitChoiceGroupItem> choices = new()
    {
        new() { Text = "Email", Value = "email" },
        new() { Text = "SMS", Value = "sms" },
        new() { Text = "Push", Value = "push" }
    };

    private async Task HandleClick(MouseEventArgs args)
    {
        // Handle click event
        Console.WriteLine("Choice clicked");
    }
}
```

### Data Binding

The ChoiceGroup supports both one-way and two-way binding:

#### Two-Way Binding (Recommended)

```razor
<BitChoiceGroup Items="options" @bind-Value="selectedValue" />

<div>Selected: @selectedValue</div>

@code {
    private string selectedValue = "default";
    private List<BitChoiceGroupItem> options = new()
    {
        new() { Text = "Option A", Value = "optA" },
        new() { Text = "Option B", Value = "optB" }
    };
}
```

#### One-Way Binding

```razor
<BitChoiceGroup Items="options"
                Value="selectedValue"
                OnChange="(value) => selectedValue = value" />
```

### Styling & CSS Customization

The ChoiceGroup provides granular control through the **BitChoiceGroupClassStyles** object:

| CSS Class/Style Target | Purpose |
|------------------------|---------|
| `Root` | Main container of the entire ChoiceGroup |
| `LabelContainer` | Container for the group label |
| `Label` | Label text styling |
| `Container` | Container for choice items |
| `ItemContainer` | Individual choice item container |
| `ItemLabel` | Label text within each choice |
| `ItemChecked` | Styling when a choice is selected |
| `ItemRadioButton` | Radio button indicator styling |
| `ItemIcon` | Icon element styling |
| `ItemIconWrapper` | Container around icons |
| `ItemImage` | Image element styling |
| `ItemImageWrapper` | Container around images |
| `ItemImageContainer` | Full image section container |
| `ItemText` | Text within each choice item |
| `ItemTextWrapper` | Container for choice text |
| `ItemPrefix` | Prefix content styling |

#### Applying Custom Styles

```razor
<!-- Component-level styling -->
<BitChoiceGroup Items="options"
                @bind-Value="selectedValue"
                Styles="customStyles"
                Classes="customClasses">
</BitChoiceGroup>

<!-- Or inline CSS -->
<BitChoiceGroup Items="options"
                @bind-Value="selectedValue"
                Style="gap: 2rem;">
</BitChoiceGroup>

@code {
    private BitChoiceGroupClassStyles customStyles = new()
    {
        Root = "custom-root-class",
        Container = "custom-container-class",
        ItemContainer = "custom-item-spacing",
        ItemLabel = "custom-label-styling"
    };

    private BitChoiceGroupClassStyles customClasses = new()
    {
        ItemChecked = "highlight-selected",
        ItemIcon = "icon-custom-size"
    };
}
```

#### Per-Item Styling with ItemTemplate

```razor
<BitChoiceGroup Items="options" @bind-Value="selectedValue">
    <ItemTemplate>
        <div class="choice-card">
            <strong>@context.Text</strong>
            <p class="choice-description">Custom description here</p>
        </div>
    </ItemTemplate>
</BitChoiceGroup>
```

### Practical Examples

#### Example 1: Theme Selection

```razor
<BitChoiceGroup Items="themeOptions"
                @bind-Value="selectedTheme"
                Horizontal="true"
                Label="Choose Your Theme"
                Size="BitSize.Large">
</BitChoiceGroup>

<div class="theme-preview">
    Current: @selectedTheme
</div>

@code {
    private List<BitChoiceGroupItem> themeOptions = new()
    {
        new() { Text = "Light", Value = "light", IconName = "Icon.Sun" },
        new() { Text = "Dark", Value = "dark", IconName = "Icon.Moon" },
        new() { Text = "Auto", Value = "auto", IconName = "Icon.Settings" }
    };

    private string selectedTheme = "light";
}
```

#### Example 2: Audio Format Selection with Images

```razor
<BitChoiceGroup Items="audioFormats"
                @bind-Value="selectedFormat"
                Inline="true"
                Label="Select Audio Format">
</BitChoiceGroup>

@code {
    private class AudioFormat
    {
        public string Name { get; set; }
        public string FormatCode { get; set; }
        public string Icon { get; set; }
    }

    private List<AudioFormat> audioFormats = new()
    {
        new() { Name = "MP3", FormatCode = "mp3", Icon = "" },
        new() { Name = "WAV", FormatCode = "wav", Icon = "" },
        new() { Name = "FLAC", FormatCode = "flac", Icon = "" },
        new() { Name = "AAC", FormatCode = "aac", Icon = "" }
    };

    private string selectedFormat = "mp3";

    private BitChoiceGroupItem.NameSelectors<AudioFormat> formatSelectors =
        new()
        {
            Text = format => format.Name,
            Value = format => format.FormatCode
        };
}
```

#### Example 3: User Preference Settings

```razor
<div class="settings-form">
    <BitChoiceGroup Items="notificationSettings"
                    @bind-Value="userNotifications"
                    Label="Notification Preferences"
                    OnChange="SaveSettings">
        <ItemTemplate>
            <div class="setting-item">
                <h5>@context.Text</h5>
                <small>@context.Value</small>
            </div>
        </ItemTemplate>
    </BitChoiceGroup>
</div>

@code {
    private List<BitChoiceGroupItem> notificationSettings = new()
    {
        new() { Text = "All Notifications", Value = "all" },
        new() { Text = "Important Only", Value = "important" },
        new() { Text = "Disabled", Value = "none" }
    };

    private string userNotifications = "important";

    private async Task SaveSettings(string preference)
    {
        // Save user preference to database
        userNotifications = preference;
        await InvokeAsync(async () =>
        {
            // Persist settings
        });
    }
}
```

#### Example 4: Horizontal Layout with Icons

```razor
<BitChoiceGroup Items="statusOptions"
                @bind-Value="selectedStatus"
                Horizontal="true"
                Label="Select Status">
</BitChoiceGroup>

@code {
    private List<BitChoiceGroupItem> statusOptions = new()
    {
        new() { Text = "Active", Value = "active", IconName = "Icon.CheckMark", Color = BitColor.Success },
        new() { Text = "Pending", Value = "pending", IconName = "Icon.Clock", Color = BitColor.Warning },
        new() { Text = "Inactive", Value = "inactive", IconName = "Icon.Cancel", Color = BitColor.Danger }
    };

    private string selectedStatus = "active";
}
```

#### Example 5: Disabled Options

```razor
<BitChoiceGroup Items="billingPlans"
                @bind-Value="selectedPlan"
                Label="Select Billing Plan">
</BitChoiceGroup>

@code {
    private List<BitChoiceGroupItem> billingPlans = new()
    {
        new() { Text = "Free Plan", Value = "free", IsEnabled = true },
        new() { Text = "Pro Plan - $9/mo", Value = "pro", IsEnabled = true },
        new() { Text = "Enterprise - Contact Sales", Value = "enterprise", IsEnabled = false }
    };

    private string selectedPlan = "free";
}
```

### Accessibility

- All options support `AriaLabel` for screen readers
- The component respects keyboard navigation (arrow keys for selection)
- Selected states are announced to assistive technologies
- Images require `ImageAlt` text for accessibility

### Performance Considerations

- For large lists (100+ items), consider lazy loading or virtualization
- Use the `Items` property for data-driven scenarios rather than hardcoded BitChoiceGroupOption components
- The component efficiently handles dynamic item updates

### Related Components

- **BitRadioGroup** - Similar component with styled radio button options
- **BitCheckbox** - For multiple selections instead of single choice
- **BitDropdown** - For larger lists requiring less screen space

---

## Dropdown

The **BitDropdown** is a powerful input component from Bit.BlazorUI that allows users to select one or more items from a list. It supports single selection, multiple selections, and combo box (searchable) modes with rich customization options.

### Component Overview

The BitDropdown displays a selected item while other options are available on demand by clicking a dropdown button. It can function as a simple single-select dropdown, a multi-select component with checkboxes, or a searchable combo box for quick item lookup.

### Selection Modes

#### Single Select (Default)
Users select one item from the dropdown list:

```csharp
<BitDropdown TItem="string" 
             Items="departments" 
             @bind-Value="selectedDepartment" 
             Placeholder="Select a department" />
```

#### Multi Select
Enable multiple selection with the `MultiSelect` parameter:

```csharp
<BitDropdown TItem="string" 
             Items="teams" 
             @bind-Values="selectedTeams" 
             MultiSelect="true" 
             Placeholder="Select teams" />
```

#### Combo Box (Searchable)
Allow users to type and search for items:

```csharp
<BitDropdown TItem="string" 
             Items="cities" 
             @bind-Value="selectedCity" 
             Combo="true"
             ShowSearchBox="true" 
             Placeholder="Type to search cities" />
```

### Defining Items

#### Option 1: Using BitDropdownItem

```csharp
var items = new List<BitDropdownItem>
{
    new BitDropdownItem { Text = "New York", Value = "NY" },
    new BitDropdownItem { Text = "Los Angeles", Value = "LA" },
    new BitDropdownItem { Text = "Chicago", Value = "CH" }
};

<BitDropdown TItem="BitDropdownItem" Items="items" />
```

#### Option 2: Using Custom Classes

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
}

var employees = new List<Employee>
{
    new Employee { Id = 1, Name = "John Doe" },
    new Employee { Id = 2, Name = "Jane Smith" },
    new Employee { Id = 3, Name = "Bob Johnson" }
};

<BitDropdown TItem="Employee" 
             Items="employees"
             @bind-Value="selectedEmployee"
             TextSelector="e => e.Name"
             ValueSelector="e => e.Id" />
```

#### Option 3: Using Template-Based Options

```razor
<BitDropdown TItem="User" Items="users" @bind-Value="selectedUser">
    <ItemTemplate>
        <span>@context.FirstName @context.LastName</span>
        <small>@context.Department</small>
    </ItemTemplate>
</BitDropdown>
```

### Key Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `ICollection<TItem>` | null | Collection of items to display in dropdown |
| `Value` | `TValue` | null | The selected item in single-select mode |
| `Values` | `IEnumerable<TValue>` | null | Collection of selected items in multi-select mode |
| `MultiSelect` | `bool` | false | Enables multiple item selection with checkboxes |
| `Combo` | `bool` | false | Activates searchable combo box mode |
| `ShowSearchBox` | `bool` | false | Displays a search input field |
| `ShowClearButton` | `bool` | false | Shows a button to clear selection(s) |
| `Chips` | `bool` | false | Displays selected items as removable chip elements |
| `Dynamic` | `bool` | false | Allows users to add new items in combo mode |
| `Responsive` | `bool` | false | Displays dropdown as side panel on small screens |
| `Label` | `string` | null | Descriptive label above the dropdown |
| `Placeholder` | `string` | null | Default prompt text when no item is selected |
| `Prefix` | `string` | null | Text or icon displayed before the input |
| `Suffix` | `string` | null | Text or icon displayed after the input |
| `Required` | `bool` | false | Makes the field required for form validation |
| `ReadOnly` | `bool` | false | Prevents user interaction |
| `Disabled` | `bool` | false | Disables the dropdown |
| `TextSelector` | `Func<TItem, string>` | null | Function to extract display text from item |
| `ValueSelector` | `Func<TItem, TValue>` | null | Function to extract value from item |

### Events and Callbacks

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnChange` | `EventCallback<TValue>` | Fires when the value changes |
| `OnSelectItem` | `EventCallback<TItem>` | Fires when an item is selected |
| `OnValuesChange` | `EventCallback<IEnumerable<TValue>>` | Fires when selected items change (multi-select) |
| `OnSearch` | `EventCallback<string>` | Fires when search text is entered |
| `OnDynamicAdd` | `EventCallback<string>` | Fires when a new item is added in dynamic mode |

### Common Usage Examples

#### Basic Single Select

```csharp
@page "/dropdown-demo"
@using Bit.BlazorUI

<BitDropdown TItem="string"
             Items="departments"
             @bind-Value="selectedDepartment"
             Label="Department"
             Placeholder="Choose a department">
</BitDropdown>

<p>Selected: @selectedDepartment</p>

@code {
    private string selectedDepartment;
    private List<string> departments = new()
    {
        "Engineering",
        "Sales",
        "Marketing",
        "HR",
        "Finance"
    };
}
```

#### Multi-Select with Chips

```csharp
<BitDropdown TItem="string"
             Items="technologies"
             @bind-Values="selectedTechs"
             MultiSelect="true"
             Chips="true"
             ShowClearButton="true"
             Label="Select Technologies"
             Placeholder="Add technologies...">
</BitDropdown>

<p>Selected count: @selectedTechs?.Count()</p>

@code {
    private IEnumerable<string> selectedTechs = new List<string>();
    private List<string> technologies = new()
    {
        "C#",
        "JavaScript",
        "TypeScript",
        "Python",
        "Java",
        ".NET"
    };
}
```

#### Searchable Combo Box

```csharp
<BitDropdown TItem="string"
             Items="cities"
             @bind-Value="selectedCity"
             Combo="true"
             ShowSearchBox="true"
             Label="City"
             Placeholder="Type to find a city">
</BitDropdown>

@code {
    private string selectedCity;
    private List<string> cities = new()
    {
        "New York",
        "Los Angeles",
        "Chicago",
        "Houston",
        "Phoenix",
        "Philadelphia",
        "San Antonio",
        "San Diego"
    };
}
```

#### Custom Item Template with Objects

```csharp
@page "/dropdown-custom"
@using Bit.BlazorUI

<BitDropdown TItem="Product"
             Items="products"
             @bind-Value="selectedProduct"
             Label="Select Product"
             TextSelector="p => p.Name">
    <ItemTemplate>
        <div style="display: flex; justify-content: space-between; align-items: center;">
            <span>@context.Name</span>
            <span style="color: #666; font-size: 0.9em;">@context.Price.ToString("C")</span>
        </div>
    </ItemTemplate>
</BitDropdown>

@if (selectedProduct != null)
{
    <p>Selected: @selectedProduct.Name - @selectedProduct.Price.ToString("C")</p>
}

@code {
    private Product selectedProduct;
    
    private List<Product> products = new()
    {
        new Product { Id = 1, Name = "Laptop", Price = 999.99m },
        new Product { Id = 2, Name = "Mouse", Price = 29.99m },
        new Product { Id = 3, Name = "Keyboard", Price = 79.99m }
    };

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
```

#### Dynamic Combo Box (User Can Add Items)

```csharp
<BitDropdown TItem="string"
             Items="tags"
             @bind-Values="selectedTags"
             MultiSelect="true"
             Combo="true"
             Dynamic="true"
             ShowSearchBox="true"
             Chips="true"
             Label="Tags"
             Placeholder="Search or add tags..."
             OnDynamicAdd="HandleDynamicAdd">
</BitDropdown>

@code {
    private List<string> tags = new() { "Bug", "Feature", "Documentation", "Help" };
    private IEnumerable<string> selectedTags = new List<string>();

    private Task HandleDynamicAdd(string newTag)
    {
        if (!tags.Contains(newTag))
        {
            tags.Add(newTag);
        }
        return Task.CompletedTask;
    }
}
```

### Customization Options

#### Labels and Text

```csharp
<BitDropdown TItem="string"
             Items="options"
             @bind-Value="selectedOption"
             Label="Choose Option"
             Placeholder="Select..."
             Prefix="🔍"
             Suffix="✓">
</BitDropdown>
```

#### Responsive Design

```csharp
<BitDropdown TItem="string"
             Items="items"
             @bind-Value="selectedItem"
             Responsive="true"
             Label="Mobile-friendly Dropdown">
</BitDropdown>
```

### CSS Customization

The `BitDropdownClassStyles` property enables styling for:

- **Root container**: Overall dropdown container styling
- **Labels**: Label text appearance
- **Text area**: Selected item display area
- **Clear button**: Clear selection button styling
- **Caret icon**: Dropdown arrow icon
- **Search box**: Search input field
- **Overlay**: Dropdown list background
- **Items**: Individual dropdown item styling
- **Dividers**: Item separator lines
- **Responsive panel**: Mobile side panel

Custom styles can be applied via the `Classes` or `Styles` parameters:

```csharp
<BitDropdown TItem="string"
             Items="items"
             @bind-Value="selectedValue"
             Classes="custom-dropdown-class"
             Style="border-color: #007bff; padding: 12px;">
</BitDropdown>
```

### Validation

The dropdown supports HTML5 and form validation:

```csharp
<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    
    <BitDropdown TItem="string"
                 Items="categories"
                 @bind-Value="model.Category"
                 Label="Category"
                 Required="true">
    </BitDropdown>
    
    <ValidationMessage For="@(() => model.Category)" />
    
    <button type="submit">Submit</button>
</EditForm>

@code {
    private FormModel model = new();

    private class FormModel
    {
        [Required(ErrorMessage = "Category is required")]
        public string Category { get; set; }
    }
}
```

### Best Practices

1. **Always provide a Placeholder** - Guides users on what to select
2. **Use Combo mode for large datasets** - Makes finding items easier
3. **Implement OnChange handlers** - React to selection changes in real-time
4. **Leverage Chips in multi-select** - Provides clear visual feedback of selections
5. **Use Custom Templates** - Display rich content beyond simple text
6. **Enable validation** - Ensure required fields are filled
7. **Consider Responsive mode** - Improves mobile experience

---

**Sources:**
- [Bit.BlazorUI Dropdown Component Documentation](https://blazorui.bitplatform.dev/components/dropdown)

---

## FileUpload

### Overview

The **BitFileUpload** component provides a robust file upload solution for Blazor applications. It wraps HTML file input elements with managed file upload capabilities, supporting single and multiple file selection, automatic uploading, file validation, chunked transfers, and progress tracking.

### Component Description

BitFileUpload manages file uploads to specified server endpoints with built-in validation for file types and sizes, optional chunked file transfer for large files, progress event callbacks, and customizable UI templates. It integrates seamlessly with the Bit.BlazorUI design system and supports both manual and automatic upload flows.

### Core Parameters

#### Basic Configuration
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `UploadUrl` | `string?` | `null` | Server endpoint URL that receives uploaded files |
| `RemoveUrl` | `string?` | `null` | Server endpoint URL for deleting uploaded files |
| `Label` | `string?` | `null` | Label text displayed for the file select button |
| `Accept` | `string?` | `null` | HTML accept attribute for file input (e.g., "image/*", ".pdf") |

#### Upload Behavior
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AutoUpload` | `bool` | `false` | Automatically initiate upload after file selection |
| `Multiple` | `bool` | `false` | Enable multiple file selection in a single action |
| `Append` | `bool` | `false` | Append newly selected files to existing list instead of replacing |
| `AutoReset` | `bool` | `false` | Automatically clear component state after successful upload |

#### File Validation
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `MaxSize` | `long` | `0` | Maximum allowed file size in bytes (0 = unlimited) |
| `AllowedExtensions` | `IReadOnlyCollection<string>` | `["*"]` | List of permitted file extensions (e.g., `["jpg", "png", "pdf"]`) |

#### Chunked Upload
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChunkedUpload` | `bool` | `false` | Enable chunked file transfer for large files |
| `ChunkSize` | `long?` | `null` | Size of each upload chunk in bytes |
| `AutoChunkSize` | `bool` | `false` | Dynamically calculate optimal chunk size between 512 KB and 10 MB |

#### UI & Customization
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowRemoveButton` | `bool` | `false` | Display remove/delete button for each uploaded file |
| `HideFileView` | `bool` | `false` | Hide the file list view section |
| `Class` | `string?` | `null` | CSS class names for styling |
| `Style` | `string?` | `null` | Inline CSS styles |

### Validation Messages

| Parameter | Default Value |
|-----------|---------------|
| `MaxSizeErrorMessage` | "The file size is larger than the max size" |
| `NotAllowedExtensionErrorMessage` | "The file type is not allowed" |
| `SuccessfulUploadMessage` | "Upload completed successfully" |
| `FailedUploadMessage` | "Upload failed" |

### Events & Callbacks

| Event | Callback Parameter | Description |
|-------|-------------------|-------------|
| `OnChange` | `BitFileInfo file` | Fires when file selection changes |
| `OnUploading` | `BitFileInfo file` | Fires before upload begins |
| `OnProgress` | `BitFileInfo file` | Fires during upload progress (use `TotalUploadedSize` / `Size` for percentage) |
| `OnUploadComplete` | `BitFileInfo file` | Fires when upload completes successfully |
| `OnUploadFailed` | `BitFileInfo file` | Fires when upload fails |
| `OnAllUploadsComplete` | `IEnumerable<BitFileInfo> files` | Fires when all files finish processing |
| `OnRemoveComplete` | `BitFileInfo file` | Fires after successful file removal |
| `OnRemoveFailed` | `BitFileInfo file` | Fires when file removal fails |

### Public Methods

| Method | Parameters | Return Type | Description |
|--------|-----------|------------|-------------|
| `Upload()` | None | `Task` | Initiates file upload to the specified UploadUrl |
| `Browse()` | None | `Task` | Opens the system file selection dialog |
| `RemoveFile(BitFileInfo file)` | `BitFileInfo` | `Task` | Removes an uploaded file (calls RemoveUrl) |
| `Reset()` | None | `Task` | Clears all files and resets component state |
| `PauseUpload()` | None | `Task` | Pauses an active upload transfer |
| `CancelUpload()` | None | `Task` | Cancels an active upload transfer |

### Templates

#### LabelTemplate
Custom Razor fragment to replace the default select button:
```razor
<LabelTemplate>
    <span>Drag and drop or <strong>click to browse</strong></span>
</LabelTemplate>
```

#### FileViewTemplate
Custom rendering for individual file items in the list:
```razor
<FileViewTemplate Context="file">
    <div class="custom-file-item">
        <span>@file.Name</span>
        <small>(@(file.Size / 1024) KB)</small>
    </div>
</FileViewTemplate>
```

---

## Practical Usage Examples

### Example 1: Basic Single File Upload

```razor
<BitFileUpload UploadUrl="https://api.example.com/upload"
               Label="Choose File"
               OnUploadComplete="HandleUploadComplete" />

@code {
    private async Task HandleUploadComplete(BitFileInfo file)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"File {file.Name} uploaded successfully!");
    }
}
```

### Example 2: Image Upload with Validation

```razor
<BitFileUpload UploadUrl="/api/images/upload"
               Label="Select Image"
               Accept="image/*"
               MaxSize="5242880"
               AllowedExtensions="@new[] { "jpg", "jpeg", "png", "gif" }"
               MaxSizeErrorMessage="Image must be smaller than 5 MB"
               NotAllowedExtensionErrorMessage="Only image files are allowed"
               AutoUpload="true"
               OnUploadComplete="OnImageUploaded"
               OnUploadFailed="OnUploadFailed" />

@code {
    private async Task OnImageUploaded(BitFileInfo file)
    {
        // Handle successful upload
    }

    private async Task OnUploadFailed(BitFileInfo file)
    {
        // Handle upload failure
    }
}
```

### Example 3: Multiple File Upload with Progress Tracking

```razor
<BitFileUpload UploadUrl="/api/files/upload"
               Multiple="true"
               Label="Select Files"
               OnProgress="HandleProgress"
               OnAllUploadsComplete="HandleAllComplete">
    <FileViewTemplate Context="file">
        <div class="file-item">
            <span>@file.Name</span>
            <span class="file-progress">
                @{
                    var percent = file.Size > 0 
                        ? ((file.TotalUploadedSize / (double)file.Size) * 100).ToString("F1")
                        : "0";
                    <text>@percent%</text>
                }
            </span>
        </div>
    </FileViewTemplate>
</BitFileUpload>

@code {
    private async Task HandleProgress(BitFileInfo file)
    {
        var uploadedPercent = (file.TotalUploadedSize / (double)file.Size) * 100;
        Console.WriteLine($"{file.Name}: {uploadedPercent:F1}% complete");
    }

    private async Task HandleAllComplete(IEnumerable<BitFileInfo> files)
    {
        Console.WriteLine($"All {files.Count()} files uploaded successfully");
    }
}
```

### Example 4: Chunked Upload for Large Files

```razor
<BitFileUpload UploadUrl="/api/files/upload"
               Label="Upload Large File"
               ChunkedUpload="true"
               AutoChunkSize="true"
               OnProgress="ShowProgress"
               OnUploadComplete="UploadComplete" />

@code {
    private async Task ShowProgress(BitFileInfo file)
    {
        var percent = (file.TotalUploadedSize / (double)file.Size) * 100;
        Console.WriteLine($"Upload progress: {percent:F1}%");
    }

    private async Task UploadComplete(BitFileInfo file)
    {
        Console.WriteLine($"Large file uploaded: {file.Name}");
    }
}
```

### Example 5: Custom UI with Drag & Drop Feel

```razor
<BitFileUpload UploadUrl="/api/documents/upload"
               Label="Select Documents"
               Multiple="true"
               AllowedExtensions="@new[] { "pdf", "docx", "xlsx" }"
               ShowRemoveButton="true"
               RemoveUrl="/api/documents/remove"
               OnRemoveComplete="HandleRemove">
    <LabelTemplate>
        <div class="upload-area">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                <polyline points="7 10 12 15 17 10"></polyline>
                <line x1="12" y1="15" x2="12" y2="3"></line>
            </svg>
            <p>Drag files here or <strong>click to browse</strong></p>
            <small>PDF, DOCX, XLSX up to 10 MB each</small>
        </div>
    </LabelTemplate>
    <FileViewTemplate Context="file">
        <div class="document-item">
            <span class="doc-name">@file.Name</span>
            <span class="doc-size">@(file.Size / 1024) KB</span>
        </div>
    </FileViewTemplate>
</BitFileUpload>

@code {
    private async Task HandleRemove(BitFileInfo file)
    {
        Console.WriteLine($"File removed: {file.Name}");
    }
}
```

### Example 6: Upload with Custom Headers (Authentication)

```razor
<BitFileUpload UploadUrl="/api/secure/upload"
               Label="Upload Secure File"
               UploadRequestHttpHeaders="@GetAuthHeaders()"
               AutoUpload="true"
               OnUploadComplete="OnSecureUpload" />

@code {
    private Dictionary<string, string> GetAuthHeaders()
    {
        return new Dictionary<string, string>
        {
            { "Authorization", "Bearer your-auth-token" },
            { "X-Custom-Header", "your-value" }
        };
    }

    private async Task OnSecureUpload(BitFileInfo file)
    {
        // Handle authenticated upload
    }
}
```

### Example 7: Append Mode (Accumulate Multiple Selections)

```razor
<BitFileUpload UploadUrl="/api/batch/upload"
               Label="Add Files"
               Multiple="true"
               Append="true"
               ShowRemoveButton="true"
               OnUploading="PreventDuplicates">
    <LabelTemplate>
        <button type="button" class="btn btn-primary">+ Add More Files</button>
    </LabelTemplate>
</BitFileUpload>

@code {
    private async Task PreventDuplicates(BitFileInfo file)
    {
        // Validation logic before upload
    }
}
```

---

## Common Use Cases

### Document Management System
Combine multiple file validation with secure endpoints and progress tracking to build a document management interface.

### Media Library
Use image file type restrictions and size limits with custom templates to create an image upload gallery.

### Bulk Import Tool
Leverage multiple file selection, chunked uploads, and the `OnAllUploadsComplete` event to batch import data from spreadsheets.

### File Attachment Feature
Integrate into forms with append mode enabled to allow users to attach multiple documents to a single submission.

---

## Styling & Customization

The BitFileUpload component supports standard Bit.BlazorUI customization:

- Use the `Class` parameter for CSS class names
- Use the `Style` parameter for inline styles
- Use `LabelTemplate` and `FileViewTemplate` for complete UI customization
- Leverage `HtmlAttributes` for additional HTML attributes

CSS class names are auto-generated following Bit.BlazorUI conventions and can be overridden in your application stylesheets.

---

This documentation provides a complete reference for implementing the BitFileUpload component in your Blazor applications, covering basic usage through advanced scenarios with chunked transfers, validation, and custom UI templates.

---

## NumberField

### Overview

The **BitNumberField** component (also known as NumberInput) is a sophisticated Blazor component for numeric data input with comprehensive customization options. It provides increment/decrement controls, flexible formatting, validation support, and accessibility features. The component is ideal for applications requiring precise numeric entry with user-friendly controls and visual feedback.

### Component Features

- **Two-way data binding** with decimal and integer value support
- **Customizable increment/decrement** controls with adjustable step intervals
- **Flexible formatting** (currency, percentages, decimals, etc.)
- **Prefix and suffix** text or custom template support
- **Multiple button layouts** (Compact, Inline, Spread)
- **Optional clear button** for quick value reset
- **Hide input option** for control-based adjustment only
- **Mouse wheel** integration with configurable direction
- **Full keyboard support** and accessibility features
- **Granular CSS customization** via BitNumberFieldClassStyles

### Component Parameters & Properties

#### Core Value Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `decimal?` | `null` | The current numeric value (supports two-way binding with `@bind-Value`) |
| `DefaultValue` | `decimal?` | `null` | Initial value when component first renders |
| `Step` | `decimal` | `1` | Increment/decrement interval for button clicks and keyboard controls |
| `Min` | `decimal?` | `null` | Minimum allowed value (no lower bound if null) |
| `Max` | `decimal?` | `null` | Maximum allowed value (no upper bound if null) |
| `Precision` | `int` | `2` | Number of decimal places for rounding and display |

#### Display & Formatting

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `NumberFormat` | `string` | `null` | .NET number format string (e.g., "N0", "N2", "C2", "P0") |
| `Prefix` | `string` | `null` | Text displayed before the numeric value |
| `Suffix` | `string` | `null` | Text displayed after the numeric value |
| `PrefixTemplate` | `RenderFragment` | `null` | Custom markup rendered as prefix (takes precedence over `Prefix`) |
| `SuffixTemplate` | `RenderFragment` | `null` | Custom markup rendered as suffix (takes precedence over `Suffix`) |
| `Placeholder` | `string` | `null` | Hint text shown when input is empty |
| `Label` | `string` | `null` | Descriptive label text for the field |
| `LabelPosition` | `BitsPosition` | `Top` | Label placement: `Top`, `Start`, `End`, or `Bottom` |
| `LabelTemplate` | `RenderFragment` | `null` | Custom markup for label rendering |

#### Button & Control Configuration

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Mode` | `BitsNumberFieldMode` | `Spread` | Button layout: `Compact` (adjacent), `Inline` (with input), `Spread` (top/bottom) |
| `ShowClearButton` | `bool` | `false` | Display a clear button to reset value to null |
| `HideInput` | `bool` | `false` | Conceal the text input field while keeping increment/decrement controls |
| `InvertMouseWheel` | `bool` | `false` | Reverse scroll direction: `true` = scroll up decreases, `false` = scroll up increases |
| `IncrementIconName` | `string` | `null` | Custom icon name for increment button (uses default up arrow if null) |
| `DecrementIconName` | `string` | `null` | Custom icon name for decrement button (uses default down arrow if null) |
| `IncrementAriaLabel` | `string` | `"Increment"` | Screen reader label for increment button |
| `DecrementAriaLabel` | `string` | `"Decrement"` | Screen reader label for decrement button |

#### State & Behavior

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Required` | `bool` | `false` | Mark field as required for validation |
| `ReadOnly` | `bool` | `false` | Prevent value modification (synonym: `IsInputReadOnly`) |
| `IsInputReadOnly` | `bool` | `false` | Alternative name for `ReadOnly` |
| `Disabled` | `bool` | `false` | Disable the entire component |
| `IconName` | `string` | `null` | Optional icon displayed in the component |

#### Accessibility & Labeling

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaLabel` | `string` | `null` | ARIA label for screen readers |
| `AriaDescription` | `string` | `null` | ARIA description text for accessibility |
| `AriaValueNow` | `string` | `null` | Current value text for ARIA |
| `AriaValueText` | `string` | `null` | Custom ARIA value representation |

#### Styling Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Classes` | `string` | `null` | Custom CSS class names |
| `Styles` | `string` | `null` | Custom inline CSS styles |
| `ClassStyles` | `BitNumberFieldClassStyles` | `null` | Granular CSS targeting object (see CSS Customization section) |

### Events & Callbacks

| Event | Type | Parameters | Description |
|-------|------|-----------|-------------|
| `OnChange` | `EventCallback<decimal?>` | New value | Triggered when value changes (from user input, buttons, or code) |
| `OnIncrement` | `EventCallback<decimal?>` | New value after increment | Fired when increment button is clicked or up arrow key is pressed |
| `OnDecrement` | `EventCallback<decimal?>` | New value after decrement | Fired when decrement button is clicked or down arrow key is pressed |
| `OnClear` | `EventCallback` | (none) | Triggered when clear button is clicked |
| `OnFocus` | `EventCallback` | (none) | Fired when input receives focus |
| `OnBlur` | `EventCallback` | (none) | Fired when input loses focus |

### Public Methods

#### FocusAsync()

Programmatically set focus to the numeric input field.

**Usage:**
```csharp
@ref="numberFieldRef"
await numberFieldRef.FocusAsync();
```

### CSS Customization

The `BitNumberFieldClassStyles` property provides granular control over component styling:

#### Available CSS Classes

```csharp
public class BitNumberFieldClassStyles
{
    public string Root { get; set; }           // Root container element
    public string Container { get; set; }       // Main input container
    public string Input { get; set; }           // Text input field
    public string Label { get; set; }           // Label element
    public string Icon { get; set; }            // Icon element
    public string ClearButton { get; set; }     // Clear button
    public string IncrementButton { get; set; } // Increment button
    public string DecrementButton { get; set; } // Decrement button
    public string IncrementIcon { get; set; }   // Increment button icon
    public string DecrementIcon { get; set; }   // Decrement button icon
    public string Prefix { get; set; }          // Prefix container
    public string Suffix { get; set; }          // Suffix container
    public string FocusedClass { get; set; }    // Applied when focused
}
```

#### Example CSS Customization

```csharp
<BitNumberField @bind-Value="price"
    ClassStyles="new BitNumberFieldClassStyles
    {
        Root = "custom-number-field",
        Input = "custom-input large-text",
        IncrementButton = "btn-green",
        DecrementButton = "btn-red"
    }" />
```

### Code Examples

#### Basic Usage

Simple integer counter with increment/decrement controls:

```razor
@page "/numberfield-basic"

<BitNumberField @bind-Value="quantity"
    Label="Quantity"
    Min="1"
    Max="100"
    Step="1" />

<p>Selected quantity: @quantity</p>

@code {
    private decimal? quantity = 5;
}
```

#### Currency Input

Format input as currency with dollar prefix:

```razor
<BitNumberField @bind-Value="price"
    Label="Product Price"
    Prefix="$"
    NumberFormat="N2"
    Min="0"
    Max="10000"
    Step="0.01"
    Placeholder="0.00" />

@code {
    private decimal? price;
}
```

#### Percentage Field

Display as percentage with custom formatting:

```razor
<BitNumberField @bind-Value="discount"
    Label="Discount (%)"
    Suffix="%"
    Min="0"
    Max="100"
    Step="5"
    Precision="0"
    NumberFormat="N0" />

@code {
    private decimal? discount = 10;
}
```

#### Temperature Control

Real-world example with temperature adjustments:

```razor
<BitNumberField @bind-Value="temperature"
    Label="Room Temperature"
    Suffix="C"
    Min="-50"
    Max="60"
    Step="0.5"
    Precision="1"
    Mode="BitsNumberFieldMode.Compact"
    OnChange="HandleTemperatureChange" />

<p>Current temperature: @temperature C</p>
<p>Comfort level: @GetComfortLevel()</p>

@code {
    private decimal? temperature = 20;

    private async Task HandleTemperatureChange(decimal? newTemp)
    {
        temperature = newTemp;
        await AdjustHVAC(temperature.Value);
    }

    private string GetComfortLevel() => temperature switch
    {
        >= 18 and <= 22 => "Comfortable",
        < 18 => "Too Cold",
        > 22 => "Too Hot",
        _ => "Unknown"
    };

    private async Task AdjustHVAC(decimal temp)
    {
        // Call API or service to adjust temperature
        Console.WriteLine($"Setting HVAC to {temp} C");
    }
}
```

#### Advanced: Custom Templates & Validation

Complex example with custom prefix/suffix templates and event handling:

```razor
<BitNumberField @bind-Value="itemCount"
    Label="Item Inventory"
    Min="0"
    Step="1"
    Required="true"
    OnChange="ValidateInventory"
    OnIncrement="LogIncrement"
    OnDecrement="LogDecrement">
    <PrefixTemplate>
        <span style="color: #0078d4; font-weight: bold;">Box</span>
    </PrefixTemplate>
    <SuffixTemplate>
        <span style="color: #50e6ff;">items</span>
    </SuffixTemplate>
</BitNumberField>

<div class="validation-message">
    @if (itemCount < 10)
    {
        <span style="color: red;">Low stock: @itemCount items</span>
    }
    else if (itemCount < 50)
    {
        <span style="color: orange;">Medium stock: @itemCount items</span>
    }
    else
    {
        <span style="color: green;">Good stock: @itemCount items</span>
    }
</div>

@code {
    private decimal? itemCount = 25;

    private async Task ValidateInventory(decimal? newCount)
    {
        itemCount = newCount;
        // Trigger API call or validation logic
        await CheckInventoryLevels();
    }

    private async Task LogIncrement(decimal? newValue)
    {
        Console.WriteLine($"Inventory increased to {newValue}");
    }

    private async Task LogDecrement(decimal? newValue)
    {
        Console.WriteLine($"Inventory decreased to {newValue}");
    }

    private async Task CheckInventoryLevels()
    {
        // Call inventory management service
    }
}
```

#### Rating/Score Input

Create a 5-star or numeric rating control:

```razor
<BitNumberField @bind-Value="rating"
    Label="Product Rating"
    Min="0"
    Max="5"
    Step="0.5"
    Precision="1"
    Mode="BitsNumberFieldMode.Inline"
    OnChange="SaveRating" />

<div class="rating-display">
    @if (rating.HasValue)
    {
        @for (int i = 0; i < (int)rating.Value; i++)
        {
            <span style="color: gold;">Star</span>
        }
        @if (rating.Value % 1 != 0)
        {
            <span style="color: gold;">Half</span>
        }
    }
</div>

@code {
    private decimal? rating;

    private async Task SaveRating(decimal? newRating)
    {
        rating = newRating;
        // Save to database or API
    }
}
```

#### Hidden Input with Controls Only

Show only increment/decrement buttons without visible input:

```razor
<BitNumberField @bind-Value="level"
    Label="Game Level"
    HideInput="true"
    Min="1"
    Max="100"
    Step="1"
    Mode="BitsNumberFieldMode.Compact" />

<p>Level: @level</p>

@code {
    private decimal? level = 1;
}
```

#### Clear Button & Reset Functionality

Enable clearing values with confirmation:

```razor
<BitNumberField @bind-Value="age"
    Label="Age"
    ShowClearButton="true"
    Min="0"
    Max="150"
    OnClear="ConfirmReset" />

@code {
    private decimal? age;

    private async Task ConfirmReset()
    {
        bool confirmed = await JsRuntime.InvokeAsync<bool>(
            "confirm", "Clear the age field?");

        if (confirmed)
        {
            age = null;
        }
    }
}
```

#### Mouse Wheel Integration

Enable scroll-based value adjustment:

```razor
<BitNumberField @bind-Value="zoom"
    Label="Zoom Level (%)"
    Min="25"
    Max="400"
    Step="25"
    Suffix="%"
    InvertMouseWheel="false"
    NumberFormat="N0" />

<p>Current zoom: @zoom%</p>

@code {
    private decimal? zoom = 100;
}
```

### Keyboard Support

| Key | Behavior |
|-----|----------|
| `Up Arrow` | Increase value by `Step` amount |
| `Down Arrow` | Decrease value by `Step` amount |
| `Ctrl + Up` | Jump to `Max` value |
| `Ctrl + Down` | Jump to `Min` value |
| `Home` | Set to `Min` value |
| `End` | Set to `Max` value |
| `Tab` | Navigate to next element |
| `Shift + Tab` | Navigate to previous element |

### Validation

The component integrates with Blazor's validation system:

```razor
<EditForm Model="@model" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />

    <BitNumberField @bind-Value="model.Price"
        Label="Price"
        Min="0"
        Max="99999" />

    <ValidationMessage For="@(() => model.Price)" />

    <button type="submit">Submit</button>
</EditForm>

@code {
    private PriceModel model = new();

    private void HandleValidSubmit()
    {
        Console.WriteLine($"Valid price: {model.Price}");
    }

    public class PriceModel
    {
        [Range(0, 99999)]
        public decimal? Price { get; set; }
    }
}
```

### Accessibility Features

The BitNumberField component supports comprehensive accessibility:

- **ARIA Labels**: `AriaLabel`, `AriaDescription` for screen readers
- **ARIA Values**: `AriaValueNow`, `AriaValueText` for current value announcement
- **Keyboard Navigation**: Full keyboard control without mouse required
- **Button Labels**: Customizable via `IncrementAriaLabel` and `DecrementAriaLabel`
- **Semantic HTML**: Proper form associations and input semantics

#### Accessibility Example

```razor
<BitNumberField @bind-Value="quantity"
    Label="Order Quantity"
    AriaLabel="Number of items to order"
    AriaDescription="Enter a quantity between 1 and 1000"
    AriaValueNow="@quantity?.ToString()"
    AriaValueText="@GetQuantityText()"
    Min="1"
    Max="1000"
    IncrementAriaLabel="Add one item"
    DecrementAriaLabel="Remove one item" />

@code {
    private decimal? quantity;

    private string GetQuantityText()
    {
        return quantity switch
        {
            null => "no quantity selected",
            1 => "one item",
            _ => $"{quantity} items"
        };
    }
}
```

### Common Patterns & Best Practices

#### 1. Form Integration
Always use two-way binding (`@bind-Value`) for automatic synchronization with your model.

#### 2. Formatting
- Use `NumberFormat` for display formatting
- Use `Precision` for rounding calculations
- Pair `Prefix`/`Suffix` with appropriate `NumberFormat`

#### 3. Validation
- Set `Min`/`Max` for constraint enforcement
- Use `Required="true"` for mandatory fields
- Integrate with Blazor's validation system for comprehensive error handling

#### 4. UX Considerations
- Choose `Mode` based on available space (Compact for tight layouts, Spread for clarity)
- Enable `ShowClearButton` for optional fields
- Use custom `PrefixTemplate`/`SuffixTemplate` for branded styling
- Provide `AriaLabel` descriptions for screen reader users

#### 5. Performance
- Avoid complex logic in `OnChange` events
- Use `@bind-Value:get` and `@bind-Value:set` for granular control if needed
- Debounce rapid increments if calling expensive operations

### Related Components

- **BitTextField**: Single-line text input
- **BitSpinButton**: Similar increment/decrement control (legacy)
- **BitSlider**: Range-based numeric selection
- **BitDropdown**: For predefined numeric selections

### Resources

- [Official Bit.BlazorUI Documentation](https://blazorui.bitplatform.dev/)
- [NumberField Component Reference](https://blazorui.bitplatform.dev/components/numberfield)
- [Bit.BlazorUI GitHub Repository](https://github.com/bitfoundation/bitplatform)

---

## OtpInput

The BitOtpInput component is a specialized Blazor UI control designed for multi-factor authentication (MFA) scenarios. It provides a user-friendly interface for entering one-time passwords (OTP) with support for automatic shifting, customizable length, and comprehensive event handling.

### Overview

The OTP input component is used for MFA procedures of authenticating users with a one-time password. It renders multiple input fields (typically 5-6 digits) and handles transitions between fields automatically, making the authentication experience seamless.

### Component Properties

#### Core Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Length` | `int` | `5` | Number of input fields to render |
| `Type` | `BitInputType` | `Text` | Input type: Text, Password, Number, Email, Tel, Url |
| `AutoFocus` | `bool` | `false` | Automatically focuses the first input field on component initialization |
| `AutoShift` | `bool` | `false` | Automatically shifts focus to next/previous field when deleting or clearing input |
| `ReadOnly` | `bool` | `false` | Makes all input fields read-only |
| `Required` | `bool` | `false` | Makes all input fields required |
| `IsEnabled` | `bool` | `true` | Enables or disables the entire component |
| `Value` | `string` | Empty | Two-way bound value containing the concatenated OTP input |

#### Visual Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Size` | `BitSize` | `Medium` | Component size: Small, Medium, Large |
| `Reversed` | `bool` | `false` | Renders input fields in reverse/opposite direction |
| `Vertical` | `bool` | `false` | Renders input fields vertically instead of horizontally |
| `Dir` | `BitDir` | `Auto` | Text direction: Ltr (left-to-right), Rtl (right-to-left), Auto |

#### Labels and Display

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Label` | `string` | null | Simple text label displayed above inputs |
| `LabelTemplate` | `RenderFragment` | null | Custom label template for advanced label customization |

### Events and Callbacks

| Event | EventArgs | Description |
|-------|-----------|-------------|
| `OnChange` | `ChangeEventArgs` | Fires when any input value changes |
| `OnFill` | `(string value)` | Fires when all input fields are successfully filled |
| `OnFocusIn` | `FocusEventArgs` | Fires when focus enters any input field |
| `OnFocusOut` | `FocusEventArgs` | Fires when focus leaves an input field |
| `OnInput` | `ChangeEventArgs` | Fires for input events on individual fields |
| `OnKeyDown` | `KeyboardEventArgs` | Fires on keyboard key down events |
| `OnPaste` | `ClipboardEventArgs` | Fires when user pastes content into an input field |

### Styling and CSS Customization

The component provides granular CSS customization through `BitOtpInputClassStyles`:

| Class Style Property | Target | Description |
|---------------------|--------|-------------|
| `Root` | Root container | Wrapper around entire component |
| `Label` | Label element | OTP input label styling |
| `InputsWrapper` | Inputs container | Wrapper around all input fields |
| `Input` | Individual input | Each OTP input field |
| `Focused` | Focused input | Currently focused input field styling |

### Public Methods and Members

| Member | Type | Description |
|--------|------|-------------|
| `InputElements` | `ElementReference[]` | Array of ElementReference objects for each input field; allows direct DOM access |
| `FocusAsync()` | `Task` | Programmatically focus a specific input element |

### Usage Examples

#### Basic OTP Input (6-digit code)

```razor
<BitOtpInput @bind-Value="otpValue"
             Length="6"
             Type="BitInputType.Number"
             Label="Enter your 6-digit code" />

@code {
    private string otpValue = string.Empty;
}
```

#### MFA Authentication with Auto-focus and Auto-shift

```razor
<BitOtpInput @bind-Value="otpCode"
             Length="6"
             Type="BitInputType.Number"
             AutoFocus="true"
             AutoShift="true"
             Label="Two-Factor Authentication"
             OnFill="HandleOtpComplete" />

@code {
    private string otpCode = string.Empty;

    private async Task HandleOtpComplete(string code)
    {
        // Verify OTP with backend
        var isValid = await AuthenticationService.VerifyOtpAsync(code);
        if (isValid)
        {
            // Proceed with login
            NavigationManager.NavigateTo("/dashboard");
        }
    }
}
```

#### OTP Input with Custom Styling

```razor
<BitOtpInput @bind-Value="otpValue"
             Length="5"
             Type="BitInputType.Number"
             Size="BitSize.Large"
             Label="Verification Code"
             Class="custom-otp-container"
             Style="margin: 20px 0;">
</BitOtpInput>

<style>
    .custom-otp-container {
        gap: 12px;
        justify-content: center;
    }

    ::deep .bit-otp-input {
        font-size: 24px;
        font-weight: bold;
        letter-spacing: 4px;
    }
</style>

@code {
    private string otpValue = string.Empty;
}
```

#### OTP Input with Event Handling and Validation

```razor
<BitOtpInput @bind-Value="otpCode"
             Length="6"
             Type="BitInputType.Number"
             AutoFocus="true"
             AutoShift="true"
             OnChange="HandleOtpChange"
             OnFill="HandleOtpFilled"
             OnKeyDown="HandleKeyDown">
</BitOtpInput>

<div class="status-message">
    @statusMessage
</div>

@code {
    private string otpCode = string.Empty;
    private string statusMessage = string.Empty;
    private int attemptCount = 0;
    private const int MaxAttempts = 3;

    private void HandleOtpChange(ChangeEventArgs e)
    {
        statusMessage = $"Current code: {otpCode}";
    }

    private async Task HandleOtpFilled(string code)
    {
        attemptCount++;

        if (attemptCount > MaxAttempts)
        {
            statusMessage = "Too many attempts. Please try again later.";
            return;
        }

        var result = await AuthService.VerifyOtpAsync(code);
        statusMessage = result.Success ? "Code verified!" : "Invalid code. Try again.";
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            otpCode = string.Empty;
            statusMessage = "OTP cleared";
        }
    }
}
```

#### RTL Language Support (Arabic/Persian)

```razor
<BitOtpInput @bind-Value="otpValue"
             Length="6"
             Type="BitInputType.Number"
             Dir="BitDir.Rtl"
             Label="Code"
             AutoFocus="true"
             AutoShift="true"
             OnFill="HandleOtpComplete" />

@code {
    private string otpValue = string.Empty;

    private async Task HandleOtpComplete(string code)
    {
        await VerifyOtpAsync(code);
    }
}
```

#### Vertical OTP Input Layout

```razor
<BitOtpInput @bind-Value="otpValue"
             Length="4"
             Type="BitInputType.Number"
             Vertical="true"
             Size="BitSize.Medium"
             AutoShift="true"
             Label="Enter PIN vertically"
             OnFill="HandleComplete" />

@code {
    private string otpValue = string.Empty;

    private async Task HandleComplete(string pin)
    {
        await VerifyPinAsync(pin);
    }
}
```

#### Password-type OTP (Hidden Characters)

```razor
<BitOtpInput @bind-Value="secureOtp"
             Length="8"
             Type="BitInputType.Password"
             AutoFocus="true"
             AutoShift="true"
             Label="Security Code (hidden)"
             OnFill="HandleSecureOtpFilled" />

@code {
    private string secureOtp = string.Empty;

    private async Task HandleSecureOtpFilled(string code)
    {
        await SecureAuthService.VerifyAsync(code);
    }
}
```

#### OTP Input with Clipboard Paste Support

```razor
<BitOtpInput @bind-Value="otpValue"
             Length="6"
             Type="BitInputType.Number"
             AutoShift="true"
             Label="Code (paste supported)"
             OnPaste="HandlePaste"
             OnFill="HandleOtpComplete" />

<p class="help-text">Tip: You can paste a 6-digit code directly</p>

@code {
    private string otpValue = string.Empty;

    private async Task HandlePaste(ClipboardEventArgs e)
    {
        // Browser automatically handles paste into individual fields
        // This event allows you to add custom logic if needed
    }

    private async Task HandleOtpComplete(string code)
    {
        var result = await AuthService.VerifyAsync(code);
        if (result.Success)
        {
            await ShowNotificationAsync("Code verified successfully");
        }
    }
}
```

### Advanced Usage Patterns

#### Resettable OTP Input with Timer

```razor
<BitOtpInput @bind-Value="otpCode"
             Length="6"
             Type="BitInputType.Number"
             AutoFocus="true"
             AutoShift="true"
             OnFill="VerifyOtp" />

<button disabled="@(!canResend)">
    @if (resendCountdown > 0)
    {
        <span>Resend code in @resendCountdown seconds</span>
    }
    else
    {
        <span>Resend Code</span>
    }
</button>

@code {
    private string otpCode = string.Empty;
    private int resendCountdown = 0;
    private bool canResend => resendCountdown == 0;

    private async Task VerifyOtp(string code)
    {
        var result = await AuthService.VerifyAsync(code);
        if (!result.Success)
        {
            otpCode = string.Empty; // Clear for retry
        }
    }

    private async Task ResendCode()
    {
        otpCode = string.Empty;
        resendCountdown = 60;

        _ = Task.Run(async () =>
        {
            while (resendCountdown > 0)
            {
                await Task.Delay(1000);
                resendCountdown--;
                StateHasChanged();
            }
        });
    }
}
```

#### Multi-Step Verification with Multiple OTP Inputs

```razor
<div class="verification-steps">
    @switch (verificationStep)
    {
        case 1:
            <h3>Email Verification</h3>
            <BitOtpInput @bind-Value="emailOtp"
                         Length="6"
                         Type="BitInputType.Number"
                         AutoFocus="true"
                         OnFill="VerifyEmailCode" />
            break;

        case 2:
            <h3>Phone Verification</h3>
            <BitOtpInput @bind-Value="phoneOtp"
                         Length="6"
                         Type="BitInputType.Number"
                         AutoFocus="true"
                         OnFill="VerifyPhoneCode" />
            break;
    }
</div>

@code {
    private int verificationStep = 1;
    private string emailOtp = string.Empty;
    private string phoneOtp = string.Empty;

    private async Task VerifyEmailCode(string code)
    {
        var result = await AuthService.VerifyEmailOtpAsync(code);
        if (result.Success)
        {
            verificationStep = 2;
            emailOtp = string.Empty;
        }
    }

    private async Task VerifyPhoneCode(string code)
    {
        var result = await AuthService.VerifyPhoneOtpAsync(code);
        if (result.Success)
        {
            await CompleteVerificationAsync();
        }
    }
}
```

### Key Features Summary

- **Automatic Field Navigation**: With `AutoShift` enabled, focus automatically moves to the next field after input and previous field on backspace
- **Flexible Length**: Configure any number of input fields from 1 to unlimited
- **Multiple Input Types**: Support for Number, Text, Password, Email, Tel, URL input types
- **Event-Rich**: Comprehensive events for change, fill, focus, keyboard, and paste interactions
- **Responsive Sizing**: Small, Medium, and Large sizes for different UI contexts
- **Internationalization**: Built-in RTL support for right-to-left languages
- **Accessibility**: Proper focus management and keyboard navigation
- **Direct DOM Access**: `InputElements` array provides access to underlying input elements when needed

### Browser Compatibility

The OtpInput component works with all modern browsers that support Blazor:
- Chrome/Edge 90+
- Firefox 88+
- Safari 14+

### Performance Considerations

- Use `AutoShift="true"` to reduce keyboard event handling complexity
- Avoid binding to `OnInput` for expensive operations; use `OnChange` or `OnFill` instead
- Consider using `OnFill` event for validation rather than `OnChange` for better performance

---

## Rating

The BitRating component enables users to display and submit ratings, helping others make more informed purchasing decisions through visual star representations.

### Component Overview

BitRating is a flexible, accessible rating control that allows users to select ratings using customizable icons, with support for multiple scales, sizes, and themes. It integrates seamlessly with Blazor forms and supports both controlled and uncontrolled binding patterns.

### Properties & Parameters

#### Core BitRating Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Max` | `int` | `5` | Maximum rating scale (e.g., 5 for 5-star rating, 10 for 10-point scale) |
| `AllowZeroStars` | `bool` | `false` | Allows setting the rating value to zero (by default, minimum is 1) |
| `SelectedIconName` | `string` | `"FavoriteStarFill"` | Icon name for selected/filled ratings |
| `UnselectedIconName` | `string` | `"FavoriteStar"` | Icon name for unselected/empty ratings |
| `Size` | `BitSize?` | `null` | Visual size of the rating component; options: `Small`, `Medium`, `Large` |
| `AriaLabelFormat` | `string?` | `null` | Accessibility label template string for standard ratings |
| `GetAriaLabel` | `Func<int, string>?` | `null` | Custom callback to generate aria-labels for readonly mode |
| `DefaultValue` | `double?` | `null` | Initial rating value for uncontrolled components |

#### Inherited from BitInputBase

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `TValue?` | `null` | Two-way binding property for the current rating value |
| `ReadOnly` | `bool` | `false` | Makes the component non-interactive/display-only |
| `Required` | `bool` | `false` | Marks the rating as required for form validation |
| `OnChange` | `EventCallback<ChangeEventArgs>` | - | Fires when the rating value changes |
| `NoValidate` | `bool` | `false` | Disables form validation for this control |

#### Inherited from BitComponentBase

| Property | Type | Description |
|----------|------|-------------|
| `IsEnabled` | `bool` | Enables or disables the component |
| `Class` | `string` | Additional CSS classes to apply |
| `Style` | `string` | Inline CSS styles |
| `Dir` | `string` | Text direction (ltr/rtl) for RTL support |
| `Visibility` | `BitVisibility` | Controls component visibility |
| `TabIndex` | `int?` | Tab order index |
| `AriaLabel` | `string` | Accessible label for screen readers |
| `Id` | `string` | Unique identifier |

### Icon Customization

The Rating component supports various icon options for customization:

**Built-in Icon Options:**
- `FavoriteStarFill` / `FavoriteStar` (default - solid stars)
- `Heart` / `HeartFill` (heart icons)
- `CheckMark` variations
- `Like` / `LikeFill` (thumbs up)
- Custom icon names from your icon library

**Example - Using Heart Icons:**
```csharp
<BitRating SelectedIconName="HeartFill" 
           UnselectedIconName="Heart" 
           Max="5" />
```

**Example - Custom Scale with Checkmarks:**
```csharp
<BitRating SelectedIconName="CheckMark" 
           UnselectedIconName="CheckmarkFill" 
           Max="10" />
```

### Common Usage Patterns

#### Basic 5-Star Rating
```csharp
<BitRating Max="5" />
```

#### Two-Way Data Binding
```csharp
@page "/rating-demo"
@using Bit.BlazorUI

<BitRating @bind-Value="currentRating" Max="5" />
<p>Current Rating: @currentRating</p>

@code {
    private int currentRating = 3;
}
```

#### Change Event Handler
```csharp
<BitRating Max="5" OnChange="HandleRatingChanged" />

@code {
    private async Task HandleRatingChanged(ChangeEventArgs e)
    {
        var newRating = (int)e.Value;
        await SubmitRating(newRating);
    }
}
```

#### Form Integration with Validation
```csharp
<EditForm Model="@model" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    
    <label>Product Rating (Required):</label>
    <BitRating @bind-Value="model.Rating" 
               Max="5" 
               Required="true" />
    
    <ValidationMessage For="@(() => model.Rating)" />
    
    <button type="submit">Submit Rating</button>
</EditForm>

@code {
    private RatingModel model = new();
    
    private async Task HandleSubmit()
    {
        await SaveRating(model);
    }
    
    public class RatingModel
    {
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }
    }
}
```

#### Read-Only Display Mode
```csharp
<BitRating Value="4" 
            Max="5" 
            ReadOnly="true" 
            AriaLabel="Product rating: 4 out of 5 stars" />
```

#### Variable Scale Rating (10-Point System)
```csharp
<BitRating @bind-Value="qualityScore" 
           Max="10" 
           SelectedIconName="FavoriteStarFill"
           UnselectedIconName="FavoriteStar" />
```

#### Allow Zero Stars
```csharp
<BitRating @bind-Value="rating" 
           Max="5" 
           AllowZeroStars="true" />
```

#### Size Variants
```csharp
<!-- Small Rating -->
<BitRating Max="5" Size="BitSize.Small" />

<!-- Medium Rating (default) -->
<BitRating Max="5" Size="BitSize.Medium" />

<!-- Large Rating -->
<BitRating Max="5" Size="BitSize.Large" />
```

#### RTL Support
```csharp
<BitRating @bind-Value="rating" 
           Max="5" 
           Dir="rtl" />
```

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<ChangeEventArgs>` | Fires when user selects a rating; provides the new value via `e.Value` |
| `GetAriaLabel` | `Func<int, string>` | Custom callback for generating accessibility labels dynamically |

### CSS Customization (BitRatingClassStyles)

The Rating component provides customizable CSS classes for styling:

| Class Property | Target | Purpose |
|----------------|--------|---------|
| `Root` | Rating container | Styles the entire rating component wrapper |
| `Button` | Individual rating buttons | Styles each clickable rating button |
| `IconContainer` | Icon wrapper | Styles the container around each icon |
| `SelectedIcon` | Selected star/icon | Styles appearance of selected ratings |
| `UnselectedIcon` | Unselected star/icon | Styles appearance of unselected ratings |

**Example - Custom Styling:**
```csharp
<BitRating Max="5" 
            Class="custom-rating"
            Style="gap: 0.5rem; padding: 1rem;" />

<style>
    :deep(.custom-rating) {
        display: flex;
        align-items: center;
    }
    
    :deep(.custom-rating .bit-rating__button) {
        transition: transform 0.2s ease;
    }
    
    :deep(.custom-rating .bit-rating__button:hover) {
        transform: scale(1.1);
    }
</style>
```

### Accessibility Features

- **Keyboard Support**: Navigate and select ratings using keyboard (arrow keys, Enter)
- **ARIA Labels**: Automatic accessibility labels (customizable via `AriaLabelFormat` or `GetAriaLabel`)
- **Screen Reader Support**: Full screen reader compatibility with descriptive labels
- **Form Integration**: Works seamlessly with Blazor's form validation and `EditForm`

### Real-World Example: Product Review Form

```csharp
@page "/review"
@using Bit.BlazorUI
@using System.ComponentModel.DataAnnotations

<h2>Submit Your Review</h2>

<EditForm Model="@reviewModel" OnValidSubmit="@SubmitReview">
    <DataAnnotationsValidator />
    <ValidationSummary />
    
    <div class="form-group">
        <label for="title">Review Title:</label>
        <InputText id="title" @bind-Value="reviewModel.Title" class="form-control" />
        <ValidationMessage For="@(() => reviewModel.Title)" />
    </div>
    
    <div class="form-group">
        <label>Overall Rating:</label>
        <BitRating @bind-Value="reviewModel.OverallRating" 
                   Max="5"
                   Required="true"
                   SelectedIconName="FavoriteStarFill"
                   UnselectedIconName="FavoriteStar"
                   Size="BitSize.Large" />
        <ValidationMessage For="@(() => reviewModel.OverallRating)" />
    </div>
    
    <div class="form-group">
        <label for="comment">Your Review:</label>
        <InputTextArea id="comment" @bind-Value="reviewModel.Comment" 
                       class="form-control" rows="4" />
        <ValidationMessage For="@(() => reviewModel.Comment)" />
    </div>
    
    <button type="submit" class="btn btn-primary">Submit Review</button>
</EditForm>

@code {
    private ReviewModel reviewModel = new();
    
    private async Task SubmitReview()
    {
        // Handle form submission
        await Task.Delay(500); // Simulate API call
        // Process reviewModel...
    }
    
    public class ReviewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(100, MinimumLength = 5)]
        public string Title { get; set; }
        
        [Range(1, 5, ErrorMessage = "Please select a rating between 1 and 5")]
        public int OverallRating { get; set; }
        
        [Required(ErrorMessage = "Comment is required")]
        [StringLength(500, MinimumLength = 10)]
        public string Comment { get; set; }
    }
}
```

### Summary

The BitRating component is a feature-rich, accessible rating control that supports:
- **Flexible Scales**: From 1 to any maximum value
- **Custom Icons**: Heart, star, checkmark, or any icon
- **Multiple Sizes**: Small, Medium, Large variants
- **Two-Way Binding**: With optional change event callbacks
- **Form Integration**: Full validation and data annotation support
- **Accessibility**: Complete ARIA support and keyboard navigation
- **RTL Support**: Proper right-to-left text direction handling
- **Customizable Styling**: CSS class hooks for complete visual control

---

## SearchBox

### Overview

The **BitSearchBox** component is an input field designed to enable users to search and locate specific items within an application. It combines a text input field with optional search suggestions, clear and search buttons, and extensive customization options for styling and behavior.

### Component Description

BitSearchBox provides a fully-featured search interface that supports:
- Real-time text input with two-way binding
- Dynamic search suggestions with filtering and debouncing
- Customizable icons, buttons, and prefixes/suffixes
- Multiple styling variants (underlined, no border, colored backgrounds)
- Accessibility features and event callbacks for search workflows

### Properties

#### Core Input Properties

| Property | Type | Description |
|----------|------|-------------|
| `Value` | `string` | Two-way bindable search text value |
| `DefaultValue` | `string` | Initial text value for uncontrolled components |
| `Placeholder` | `string` | Hint text displayed when field is empty |
| `ReadOnly` | `bool` | Prevents user editing (default: false) |
| `Required` | `bool` | Marks field as mandatory (default: false) |
| `AutoFocus` | `bool` | Automatically focuses element on page load (default: false) |

#### Visual Styling Properties

| Property | Type | Description |
|----------|------|-------------|
| `Background` | `BitColorKind` | Background color variant |
| `Color` | `BitColor` | General component coloring |
| `Underlined` | `bool` | Shows underline-only style instead of bordered box |
| `NoBorder` | `bool` | Removes default border styling |
| `DisableAnimation` | `bool` | Disables icon animations (default: false) |

#### Icon Configuration

| Property | Type | Description |
|----------|------|-------------|
| `IconName` | `string` | Search icon identifier |
| `HideIcon` | `bool` | Toggles search icon visibility (default: false) |
| `FixedIcon` | `bool` | Keeps icon visible while focused (default: false) |

#### Search Suggestions

| Property | Type | Description |
|----------|------|-------------|
| `SuggestItems` | `ICollection<string>` | List of suggested values to display |
| `SuggestItemTemplate` | `RenderFragment` | Custom template for rendering suggestion items |
| `MaxSuggestCount` | `int` | Maximum number of suggestions to display (default: 5) |
| `MinSuggestTriggerChars` | `int` | Minimum characters required before suggestions appear (default: 3) |
| `SuggestFilterFunction` | `Func<string, ICollection<string>, Task<ICollection<string>>>` | Custom filtering logic for suggestions (replaces default matching) |
| `SuggestItemsProvider` | `Func<string, Task<ICollection<string>>>` | Async function providing dynamic suggestions based on input |
| `DebounceTime` | `int` | Millisecond delay before filtering suggestions (default: varies) |
| `Modeless` | `bool` | Removes overlay styling for suggestions display |
| `FixedCalloutWidth` | `bool` | Constrains suggestion box to component width |

#### Clear Button

| Property | Type | Description |
|----------|------|-------------|
| `HideClearButton` | `bool` | Hides clear button when text is present (default: false) |
| `ClearButtonTemplate` | `RenderFragment` | Custom template for clear button icon |

#### Search Button

| Property | Type | Description |
|----------|------|-------------|
| `ShowSearchButton` | `bool` | Displays search submission button (default: false) |
| `SearchButtonIconName` | `string` | Custom icon for search button |
| `SearchButtonTemplate` | `RenderFragment` | Custom template for search button rendering |

#### Input Affixes

| Property | Type | Description |
|----------|------|-------------|
| `Prefix` | `string` | Text prepended to input (not included in value) |
| `PrefixTemplate` | `RenderFragment` | Custom template for prefix rendering |
| `Suffix` | `string` | Text appended to input (not included in value) |
| `SuffixTemplate` | `RenderFragment` | Custom template for suffix rendering |

#### Input Behavior

| Property | Type | Description |
|----------|------|-------------|
| `Immediate` | `bool` | Updates on input event rather than change event |
| `InputMode` | `BitInputMode` | Mobile keyboard type (text, numeric, email, url, tel, search, decimal) |
| `AutoComplete` | `string` | HTML autocomplete attribute value |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<string>` | Fired when the search value changes |
| `OnSearch` | `EventCallback<string>` | Triggered when Enter key is pressed |
| `OnClear` | `EventCallback` | Executed when clear button is clicked or Escape key is pressed |
| `OnEscape` | `EventCallback` | Triggered when Escape key is pressed |

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| `InputElement` | `ElementReference` | Reference to underlying HTML input element |
| `FocusAsync()` | `Task` | Programmatically focus the search box |

### CSS Customization

The component provides the following CSS class properties via `BitSearchBoxClassStyles`:

- **Root styling**: `Root`, `Focused`
- **Input area**: `InputContainer`, `Input`
- **Icons**: `IconWrapper`, `Icon`
- **Affixes**: `PrefixContainer`, `Prefix`, `SuffixContainer`, `Suffix`
- **Buttons**: `ClearButton`, `ClearButtonIcon`, `SearchButton`, `SearchButtonIcon`
- **Suggestions**: `Overlay`, `Callout`, `ScrollContainer`, `SuggestItemWrapper`, `SuggestItemButton`, `SuggestItemText`

### Code Examples

#### Basic Search Box

```razor
<BitSearchBox Placeholder="Search items..." />
```

#### Two-Way Data Binding

```razor
<BitSearchBox @bind-Value="@searchText" 
             Placeholder="Search..." 
             @onchange="@OnSearchChange" />

@code {
    private string searchText = "";

    private void OnSearchChange(ChangeEventArgs e)
    {
        searchText = e.Value?.ToString() ?? "";
    }
}
```

#### Search with Static Suggestions

```razor
<BitSearchBox @bind-Value="@searchText"
             SuggestItems="@suggestions"
             MaxSuggestCount="5"
             MinSuggestTriggerChars="2"
             Placeholder="Search countries..." />

@code {
    private string searchText = "";
    private ICollection<string> suggestions = new[] 
    { 
        "United States", "United Kingdom", "Canada", "Australia", "Germany" 
    };
}
```

#### Search with Dynamic Async Suggestions

```razor
<BitSearchBox @bind-Value="@searchText"
             SuggestItemsProvider="@GetSuggestions"
             DebounceTime="300"
             MinSuggestTriggerChars="3"
             Placeholder="Search products..." />

@code {
    private string searchText = "";

    private async Task<ICollection<string>> GetSuggestions(string input)
    {
        // Simulate API call
        await Task.Delay(100);
        
        var allProducts = new[] 
        { 
            "Laptop Computer", "Laptop Stand", "Laptop Bag", 
            "Desktop PC", "Desktop Stand", "Monitor"
        };
        
        return allProducts
            .Where(p => p.Contains(input, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
```

#### Search with Custom Filter Function

```razor
<BitSearchBox @bind-Value="@searchText"
             SuggestItems="@items"
             SuggestFilterFunction="@CustomFilter"
             Placeholder="Search..." />

@code {
    private string searchText = "";
    private ICollection<string> items = new[] 
    { 
        "Apple", "Apricot", "Banana", "Blueberry", "Orange" 
    };

    private Task<ICollection<string>> CustomFilter(
        string input, 
        ICollection<string> items)
    {
        // Custom filtering: starts with match only
        var filtered = items
            .Where(i => i.StartsWith(input, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return Task.FromResult<ICollection<string>>(filtered);
    }
}
```

#### Search with Clear and Search Buttons

```razor
<BitSearchBox @bind-Value="@searchText"
             Placeholder="What are you looking for?"
             ShowSearchButton="true"
             OnSearch="@HandleSearch"
             OnClear="@HandleClear"
             SearchButtonIconName="Search" />

@code {
    private string searchText = "";

    private async Task HandleSearch(string query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            // Execute search logic
            Console.WriteLine($"Searching for: {query}");
            await Task.Delay(500);
        }
    }

    private void HandleClear()
    {
        searchText = "";
        Console.WriteLine("Search cleared");
    }
}
```

#### Styled Search Box with Prefix/Suffix

```razor
<BitSearchBox @bind-Value="@searchText"
             Placeholder="Enter amount"
             Prefix="$"
             Suffix=".00"
             Color="BitColor.Primary"
             Underlined="true" />

@code {
    private string searchText = "";
}
```

#### Custom Template for Suggestions

```razor
<BitSearchBox @bind-Value="@searchText"
             SuggestItems="@products"
             Placeholder="Search products...">
    <SuggestItemTemplate>
        <div class="custom-suggestion">
            <strong>@context</strong>
            <span class="price">In Stock</span>
        </div>
    </SuggestItemTemplate>
</BitSearchBox>

@code {
    private string searchText = "";
    private ICollection<string> products = new[] 
    { 
        "Product A", "Product B", "Product C" 
    };
}
```

#### Programmatic Focus Control

```razor
<BitSearchBox @ref="@searchBoxRef"
             Placeholder="Click button to focus me..." />

<BitButton OnClick="@FocusSearch">Focus Search Box</BitButton>

@code {
    private BitSearchBox searchBoxRef;

    private async Task FocusSearch()
    {
        await searchBoxRef.FocusAsync();
    }
}
```

#### Real-World: Product Search with Debouncing

```razor
<BitSearchBox @bind-Value="@searchQuery"
             SuggestItemsProvider="@SearchProducts"
             DebounceTime="400"
             MinSuggestTriggerChars="2"
             Placeholder="Search products..."
             ShowSearchButton="true"
             OnSearch="@PerformFullSearch"
             IconName="Search" />

@code {
    private string searchQuery = "";

    private async Task<ICollection<string>> SearchProducts(string query)
    {
        // Simulate API call with delay
        await Task.Delay(200);
        
        var mockProducts = new[] 
        { 
            "Wireless Mouse", "Wireless Keyboard", "Wireless Headphones",
            "USB Cable", "USB Hub", "USB-C Adapter",
            "Monitor Stand", "Laptop Stand", "Phone Stand"
        };
        
        return mockProducts
            .Where(p => p.Contains(query, StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p)
            .Take(5)
            .ToList();
    }

    private async Task PerformFullSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query)) return;
        
        // Navigate to results page or trigger full search
        Console.WriteLine($"Full search for: {query}");
        await Task.Delay(500);
    }
}
```

#### Read-Only Search Display

```razor
<BitSearchBox Value="@currentSearch"
             ReadOnly="true"
             Placeholder="Search results..."
             HideClearButton="true"
             HideIcon="false" />

@code {
    private string currentSearch = "Active Filters Applied";
}
```

### Best Practices

1. **Use DebounceTime for API Calls**: When using `SuggestItemsProvider` with API calls, set `DebounceTime` to reduce server requests (typically 300-500ms)

2. **Set MinSuggestTriggerChars**: Require a minimum number of characters (typically 2-3) before showing suggestions to improve performance

3. **Custom Filter Function**: Use `SuggestFilterFunction` for complex filtering logic that can't be achieved with simple string matching

4. **Handle Search Events**: Use `OnSearch` event for Enter key submission and `OnClear` for cleanup operations

5. **Provide Meaningful Placeholder**: Clear placeholder text helps users understand the search purpose

6. **Mobile Optimization**: Use `InputMode="BitInputMode.Search"` on mobile devices for appropriate keyboard

7. **Accessibility**: Always include descriptive `Placeholder` text and use proper ARIA labels if needed

8. **Performance**: Use `@ref` to get element references only when needed for programmatic focus

---

## Slider

The **BitSlider** component provides a visual input control for selecting values within a defined range. It supports both single-value and dual-thumb range selection modes, with extensive customization options for styling, behavior, and accessibility.

### Component Overview

BitSlider is a versatile input control that allows users to select one or more values along a continuous range. The component provides visual feedback through value labels, supports vertical and horizontal orientations, and includes comprehensive styling options for integration into any design system.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `double` | `0` | The primary slider value for single-slider mode |
| `Min` | `double` | `0` | Minimum selectable value on the range |
| `Max` | `double` | `10` | Maximum selectable value on the range |
| `Step` | `double` | `1` | Increment between adjacent values when dragging or using arrow keys |
| `IsRanged` | `bool` | `false` | Enables dual-thumb range selection mode |
| `IsVertical` | `bool` | `false` | Renders the slider vertically instead of horizontally |
| `IsReadOnly` | `bool` | `false` | Prevents user interaction while displaying the current value |
| `ShowValue` | `bool` | `true` | Displays the current value as a label above the thumb |
| `Label` | `string?` | `null` | Descriptive label text displayed above the slider |
| `ValueFormat` | `string?` | `null` | Format string for displaying the value label |
| `IsOriginFromZero` | `bool` | `false` | Attaches the slider fill bar to the zero point instead of the start |

### Range Slider Properties

When `IsRanged` is set to `true`, use these properties to manage the dual-thumb range:

| Property | Type | Description |
|----------|------|-------------|
| `LowerValue` | `double` | The minimum value of the selected range |
| `UpperValue` | `double` | The maximum value of the selected range |
| `DefaultLowerValue` | `double?` | Initial lower value (if not set separately) |
| `DefaultUpperValue` | `double?` | Initial upper value (if not set separately) |
| `RangeValue` | `BitSliderRangeValue?` | Object containing both lower and upper bounds simultaneously |

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback` | Fires each time the slider value changes during user interaction or programmatic updates |
| `AriaValueText` | `Func<double, string>?` | Callback function to provide screen reader descriptions for the current value |

### Usage Examples

#### Basic Single Slider

```razor
@page "/slider-basic"

<BitSlider @bind-Value="sliderValue" 
           Min="0" 
           Max="100" 
           Step="5"
           Label="Volume Control" />

<p>Current Value: @sliderValue</p>

@code {
    private double sliderValue = 50;
}
```

#### Range Slider

```razor
@page "/slider-range"

<BitSlider IsRanged="true"
           @bind-LowerValue="lowerValue"
           @bind-UpperValue="upperValue"
           Min="0"
           Max="1000"
           Step="10"
           Label="Price Range" />

<p>Price Range: $@lowerValue - $@upperValue</p>

@code {
    private double lowerValue = 100;
    private double upperValue = 500;
}
```

#### Vertical Slider

```razor
@page "/slider-vertical"

<BitSlider IsVertical="true"
           @bind-Value="brightness"
           Min="0"
           Max="100"
           Label="Brightness" />

<p>Brightness: @brightness%</p>

@code {
    private double brightness = 75;
}
```

#### Value with Formatting

```razor
@page "/slider-formatted"

<BitSlider @bind-Value="temperature"
           Min="-20"
           Max="40"
           Step="0.5"
           Label="Temperature"
           ValueFormat="0.0°C" />

<p>Current Temperature: @temperature.ToString("0.0")°C</p>

@code {
    private double temperature = 20.0;
}
```

#### Read-Only Display

```razor
@page "/slider-readonly"

<BitSlider Value="75"
           IsReadOnly="true"
           Min="0"
           Max="100"
           Label="Progress"
           ShowValue="true" />
```

#### With OnChange Event

```razor
@page "/slider-events"

<BitSlider @bind-Value="volume"
           Min="0"
           Max="100"
           Step="1"
           Label="Audio Volume"
           OnChange="@HandleVolumeChange" />

<p>Volume: @volume (@volumePercent%)</p>

@code {
    private double volume = 50;
    private string volumePercent = "50";

    private async Task HandleVolumeChange()
    {
        volumePercent = $"{volume:F0}";
        // Perform additional logic when volume changes
        await InvokeAsync(StateHasChanged);
    }
}
```

#### Origin from Zero

```razor
@page "/slider-origin"

<BitSlider @bind-Value="signedValue"
           Min="-50"
           Max="50"
           IsOriginFromZero="true"
           Label="Balance" />

<p>Balance: @(signedValue > 0 ? "Right " + signedValue : signedValue < 0 ? "Left " + Math.Abs(signedValue) : "Center")</p>

@code {
    private double signedValue = 0;
}
```

#### Accessibility with Screen Reader Text

```razor
@page "/slider-accessible"

<BitSlider @bind-Value="rating"
           Min="1"
           Max="5"
           Step="1"
           Label="Product Rating"
           AriaValueText="@GetRatingDescription" />

<p>Rating: @rating Stars</p>

@code {
    private double rating = 3;

    private string GetRatingDescription(double value)
    {
        return value switch
        {
            1 => "Poor",
            2 => "Fair",
            3 => "Good",
            4 => "Very Good",
            5 => "Excellent",
            _ => "Unknown"
        };
    }
}
```

### CSS Customization

The **BitSliderClassStyles** property allows customization of the following CSS classes and elements:

| Class | Description |
|-------|-------------|
| `Root` | Main container for the entire slider component |
| `Label` | Styling for the descriptive label text |
| `Container` | Internal container for the slider track and thumb |
| `SliderBox` | The track/rail element |
| `ValueLabel` | Label displaying the current single value |
| `LowerValueLabel` | Label for the lower value in range mode |
| `UpperValueLabel` | Label for the upper value in range mode |
| `ValueInput` | Hidden input element for single-value mode |
| `LowerValueInput` | Hidden input element for lower range value |
| `UpperValueInput` | Hidden input element for upper range value |
| `OriginFromZero` | Styling for the origin-from-zero fill attachment point |

#### Custom Styling Example

```razor
@page "/slider-custom-style"

<BitSlider @bind-Value="customValue"
           Min="0"
           Max="100"
           Label="Custom Styled"
           ClassStyles="@customStyles" />

@code {
    private double customValue = 50;
    
    private BitSliderClassStyles customStyles = new()
    {
        Root = "custom-slider-root",
        Label = "custom-slider-label",
        Container = "custom-slider-container",
        SliderBox = "custom-slider-box",
        ValueLabel = "custom-value-label"
    };
}
```

### RTL Support

The BitSlider component respects RTL (Right-to-Left) layout through the `Dir` property, automatically adjusting the slider orientation for languages that read right to left.

```razor
<BitSlider @bind-Value="value" Dir="ltr" />  <!-- Left-to-right -->
<BitSlider @bind-Value="value" Dir="rtl" />  <!-- Right-to-left -->
```

### Visibility States

The component supports visibility control through standard states:

- **Visible** (default): Component is displayed normally
- **Hidden**: Component is hidden but takes up layout space
- **Collapsed**: Component is hidden and takes no layout space

### Common Use Cases

1. **Volume/Media Controls**: Use a horizontal slider with 0-100 range for audio/video volume adjustment
2. **Price Filters**: Implement range sliders for e-commerce price filtering with currency formatting
3. **Settings Panels**: Vertical sliders work well for brightness, contrast, or other intensity settings
4. **Progress Indicators**: Display progress with read-only sliders showing completion percentage
5. **Balance Controls**: Use `IsOriginFromZero` for centered controls like audio balance or color adjustments

---

**Note**: The BitSlider component is fully accessible with ARIA support, keyboard navigation (arrow keys), and screen reader compatibility through the `AriaValueText` callback function.

---

## TextField

The **BitTextField** component provides a flexible, accessible text input field for Blazor applications. It supports various input types, multi-line editing, prefixes/suffixes, icons, and comprehensive styling options.

### Component Description

BitTextField enables users to enter and edit text across forms, dialogs, tables, and other interfaces requiring text input. It offers single-line and multi-line variants with support for passwords, numbers, emails, and custom input types.

### Core Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Label` | string | Text displayed above the input field |
| `Placeholder` | string | Hint text shown when field is empty |
| `Value` | TValue | Two-way bound current input value |
| `Type` | BitInputType | Input type (Text, Password, Number, Email, Tel, Url) |
| `Multiline` | bool | Enables multi-line textarea mode |
| `ReadOnly` | bool | Prevents user modification |
| `Required` | bool | Marks field as mandatory |
| `MaxLength` | int | Maximum character limit |
| `MinLength` | int | Minimum character requirement |
| `AutoFocus` | bool | Auto-focuses field on component render |
| `Trim` | bool | Automatically removes leading/trailing whitespace |

### Visual & Styling Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `Underlined` | bool | Removes standard border, shows underline only |
| `NoBorder` | bool | Borderless appearance |
| `FullWidth` | bool | Stretches to 100% of container width |
| `Accent` | BitColor | Color applied to focus/active state |
| `Background` | BitColorKind | Background color variant |
| `Border` | BitColorKind | Border color variant |
| `ClassName` | string | Additional CSS class names |
| `Style` | string | Inline CSS styles |

### Prefix & Suffix

| Parameter | Type | Description |
|-----------|------|-------------|
| `Prefix` | string | Non-editable text displayed before input |
| `Suffix` | string | Non-editable text displayed after input |
| `IconName` | string | Icon displayed on the right side of field |

### Advanced Features

| Parameter | Type | Description |
|-----------|------|-------------|
| `ShowClearButton` | bool | Displays clear button when field has value |
| `CanRevealPassword` | bool | Shows password visibility toggle (for password type) |
| `InputMode` | BitInputMode | Mobile keyboard optimization (Text, Decimal, Numeric, Tel, Search, Email, Url) |
| `Disabled` | bool | Disables the input field |

### Event Callbacks

| Event | Signature | Description |
|-------|-----------|-------------|
| `OnChange` | `EventCallback<ChangeEventArgs>` | Fires when value is modified |
| `OnEnter` | `EventCallback<KeyboardEventArgs>` | Fires when Enter key is pressed |
| `OnFocus` | `EventCallback<FocusEventArgs>` | Fires when field receives focus |
| `OnFocusOut` | `EventCallback<FocusEventArgs>` | Fires when field loses focus |
| `OnKeyDown` | `EventCallback<KeyboardEventArgs>` | Fires on keyboard key down |
| `OnKeyUp` | `EventCallback<KeyboardEventArgs>` | Fires on keyboard key up |
| `OnClear` | `EventCallback<MouseEventArgs>` | Fires when clear button is clicked |

### Input Type Options

The `BitInputType` enum provides standard HTML5 input types:

- **Text**: Standard single-line text (default)
- **Password**: Masked password input
- **Number**: Numeric input with spinner
- **Email**: Email validation
- **Tel**: Telephone number format
- **Url**: URL validation

### CSS Customization

Use `BitTextFieldClassStyles` to customize the appearance of component sub-elements:

- **Root**: Outer container
- **Label**: Label text styling
- **Input**: Input field itself
- **Icon**: Icon element
- **Prefix**: Prefix text styling
- **Suffix**: Suffix text styling
- **Description**: Helper/error text
- **ClearButton**: Clear button styling

### Common Usage Examples

#### Basic Text Input

```razor
<BitTextField Label="Username" 
              Placeholder="Enter your username"
              @bind-Value="username" />
```

#### Password Field with Reveal Toggle

```razor
<BitTextField Label="Password"
              Type="BitInputType.Password"
              CanRevealPassword="true"
              @bind-Value="password"
              Required="true" />
```

#### Email Input with Validation

```razor
<BitTextField Label="Email Address"
              Type="BitInputType.Email"
              Placeholder="user@example.com"
              @bind-Value="email"
              Required="true" />
```

#### Multi-line Text Area

```razor
<BitTextField Label="Comments"
              Placeholder="Enter your feedback here"
              Multiline="true"
              MaxLength="500"
              @bind-Value="comments" />
```

#### TextField with Prefix/Suffix

```razor
<BitTextField Label="Price"
              Type="BitInputType.Number"
              Prefix="$"
              Placeholder="0.00"
              @bind-Value="price" />

<BitTextField Label="Website"
              Prefix="https://"
              Suffix=".com"
              Placeholder="example"
              @bind-Value="domain" />
```

#### Search Field with Clear Button

```razor
<BitTextField Label="Search"
              Placeholder="Search items..."
              ShowClearButton="true"
              @bind-Value="searchQuery"
              OnClear="@(async () => { searchQuery = null; })" />
```

#### Underlined Style

```razor
<BitTextField Label="Full Name"
              Underlined="true"
              Placeholder="Enter full name"
              @bind-Value="fullName" />
```

#### Read-only Display

```razor
<BitTextField Label="Account ID"
              Value="ACC-12345-XYZ"
              ReadOnly="true" />
```

#### Telephone Input with Mobile Keyboard

```razor
<BitTextField Label="Phone Number"
              Type="BitInputType.Tel"
              InputMode="BitInputMode.Tel"
              Placeholder="(555) 000-0000"
              @bind-Value="phoneNumber" />
```

#### Number Input with Limits

```razor
<BitTextField Label="Quantity"
              Type="BitInputType.Number"
              Placeholder="0"
              MaxLength="3"
              @bind-Value="quantity"
              OnChange="@((ChangeEventArgs e) => ValidateQuantity((string)e.Value))" />
```

#### Full-width Form Field

```razor
<BitTextField Label="Message"
              FullWidth="true"
              Multiline="true"
              Placeholder="Type your message here..."
              @bind-Value="message"
              OnEnter="@(async () => { await SendMessage(); })" />
```

#### Styled with Custom Colors

```razor
<BitTextField Label="Username"
              Placeholder="Enter username"
              Accent="BitColor.Primary"
              Background="BitColorKind.Light"
              @bind-Value="username" />
```

### Practical Integration Example

```razor
@page "/user-profile"
@using YourNamespace.Models

<div class="form-container">
    <BitTextField Label="Display Name"
                  Placeholder="Enter your display name"
                  Required="true"
                  MaxLength="50"
                  @bind-Value="userProfile.DisplayName" />

    <BitTextField Label="Email"
                  Type="BitInputType.Email"
                  Placeholder="your@email.com"
                  Required="true"
                  @bind-Value="userProfile.Email"
                  OnChange="@ValidateEmail" />

    <BitTextField Label="Bio"
                  Multiline="true"
                  Placeholder="Tell us about yourself..."
                  MaxLength="200"
                  @bind-Value="userProfile.Bio" />

    <BitTextField Label="Website"
                  Type="BitInputType.Url"
                  Prefix="https://"
                  Placeholder="example.com"
                  @bind-Value="userProfile.Website" />
</div>

@code {
    private UserProfile userProfile = new();

    private async Task ValidateEmail(ChangeEventArgs args)
    {
        // Perform async validation
        await Task.CompletedTask;
    }
}
```

### Key Features Summary

- **Flexible Input Types**: Text, Password, Email, Phone, URL, Number
- **Multi-line Support**: Textarea mode for longer text
- **Clear Button**: Auto-clear functionality for better UX
- **Password Reveal**: Optional password visibility toggle
- **Prefix/Suffix**: Display units, currency, or protocol indicators
- **Icon Support**: Right-aligned icon indicators
- **Mobile Optimization**: InputMode for better mobile keyboard experience
- **Full Accessibility**: ARIA support and semantic HTML
- **Extensive Styling**: Underlined, borderless, and color customization options

This component is ideal for contact forms, login screens, search interfaces, and any scenario requiring flexible text input in Blazor applications.

---

## Toggle

The **BitToggle** component (also known as Switch) is a control that allows users to toggle between two mutually exclusive states—typically "On/Off" or "Show/Hide"—with results appearing immediately upon selection. It provides a familiar physical switch metaphor for boolean choice.

### Component Description

BitToggle represents a binary choice control that delivers immediate visual feedback. It's ideal for settings, feature toggles, and any scenario where users need to switch between two opposite states.

### Core Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Value` | `bool?` | `null` | Gets or sets the current toggle state (supports two-way binding with `@bind-Value`) |
| `DefaultValue` | `bool?` | `null` | Sets the initial toggle state when `Value` hasn't been assigned |
| `OnText` | `string?` | `null` | Text displayed when the toggle is in the "on" (activated) state |
| `OffText` | `string?` | `null` | Text displayed when the toggle is in the "off" (deactivated) state |
| `Text` | `string?` | `null` | Fallback text displayed when both `OnText` and `OffText` are unspecified |
| `Label` | `string?` | `null` | Text label for the toggle control |
| `LabelTemplate` | `RenderFragment?` | `null` | Custom template for rendering the label instead of simple text |
| `Inline` | `bool` | `false` | When `true`, renders the label and knob on a single line |
| `FullWidth` | `bool` | `false` | When `true`, expands the toggle to fill container width with space between label and knob |
| `Disabled` | `bool` | `false` | Prevents user interaction with the toggle |
| `ReadOnly` | `bool` | `false` | Displays the toggle but prevents modification |
| `Required` | `bool` | `false` | Marks the field as required for form validation |
| `Role` | `string` | `"switch"` | ARIA role designation (typically "switch" for accessibility) |
| `NoValidate` | `bool` | `false` | Disables form validation for this component |

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<bool>` | Triggered when the toggle state changes; receives the new state value |

### Layout Options

**Inline Layout:**
Renders the label and toggle knob on a single line for compact vertical space usage.

**FullWidth Layout:**
Expands the toggle to fill available container width, with the label on the left and toggle on the right, separated by space.

### CSS Customization

The BitToggle component supports fine-grained styling through **BitToggleClassStyles** properties, allowing custom CSS classes or inline styles on these sections:

- **Root** — The outermost container element
- **Label** — The label text area
- **Container** — The main toggle wrapper
- **Button** — The interactive switch button
- **Checked** — Applied when toggle is in "on" state
- **Thumb** — The sliding knob indicator
- **Text** — The On/Off text display area

### Basic Usage

```razor
@page "/toggle-demo"

<BitToggle @bind-Value="isEnabled" Label="Enable Feature" />

<p>Current state: @isEnabled</p>

@code {
    private bool isEnabled = false;
}
```

### With On/Off Text

```razor
<BitToggle @bind-Value="isVisible" 
           OnText="Show" 
           OffText="Hide" 
           Label="Visibility Toggle" />
```

### Inline Layout

```razor
<BitToggle @bind-Value="isDarkMode" 
           Label="Dark Mode" 
           Inline="true" />
```

### FullWidth Layout

```razor
<div style="width: 100%; max-width: 400px;">
    <BitToggle @bind-Value="isNotified" 
               Label="Enable Notifications" 
               FullWidth="true" />
</div>
```

### With Custom Label Template

```razor
<BitToggle @bind-Value="isSubscribed">
    <LabelTemplate>
        <span style="font-weight: bold;">Subscribe to Updates</span>
    </LabelTemplate>
</BitToggle>
```

### Form Integration with Validation

```razor
<EditForm Model="@userSettings" OnValidSubmit="@HandleValidSubmit">
    <DataAnnotationsValidator />
    
    <BitToggle @bind-Value="userSettings.AcceptTerms" 
               Label="Accept Terms and Conditions" 
               Required="true" />
    
    <ValidationMessage For="@(() => userSettings.AcceptTerms)" />
    
    <button type="submit" class="btn btn-primary">Save Settings</button>
</EditForm>

@code {
    private UserSettings userSettings = new();
    
    private void HandleValidSubmit()
    {
        // Handle form submission
    }
    
    public class UserSettings
    {
        [Required(ErrorMessage = "You must accept the terms")]
        public bool AcceptTerms { get; set; }
    }
}
```

### Disabled State

```razor
<BitToggle @bind-Value="isProcessing" 
           Label="Processing" 
           Disabled="@isProcessing" />

@code {
    private bool isProcessing = true;
}
```

### Change Event Handling

```razor
<BitToggle Label="Feature Toggle" 
           OnChange="@OnToggleChanged" />

<p>@statusMessage</p>

@code {
    private string statusMessage = "Feature is Off";
    
    private async Task OnToggleChanged(bool newValue)
    {
        statusMessage = newValue ? "Feature is On" : "Feature is Off";
        // Perform async operations based on new state
        await NotifyServerAsync(newValue);
    }
    
    private async Task NotifyServerAsync(bool state)
    {
        // Call API to persist state
    }
}
```

### Accessibility Features

- **ARIA Support:** The component uses the "switch" ARIA role by default for proper screen reader identification
- **RTL Support:** Full right-to-left language support for RTL cultures
- **Keyboard Navigation:** Standard keyboard interaction patterns for toggle controls
- **Label Association:** Properly associates labels with the toggle for form accessibility

### Real-World Example: Settings Panel

```razor
@page "/user-settings"

<div class="settings-panel">
    <h3>User Preferences</h3>
    
    <section class="setting-group">
        <BitToggle @bind-Value="settings.DarkMode" 
                   Label="Dark Mode" 
                   FullWidth="true" />
    </section>
    
    <section class="setting-group">
        <BitToggle @bind-Value="settings.EmailNotifications" 
                   OnText="Enabled" 
                   OffText="Disabled"
                   Label="Email Notifications" 
                   FullWidth="true" />
    </section>
    
    <section class="setting-group">
        <BitToggle @bind-Value="settings.TwoFactor" 
                   Label="Two-Factor Authentication" 
                   FullWidth="true" />
    </section>
    
    <button class="btn btn-primary" @onclick="SaveSettings">Save Settings</button>
</div>

<style>
    .settings-panel {
        max-width: 500px;
        padding: 20px;
    }
    
    .setting-group {
        margin-bottom: 20px;
        padding-bottom: 15px;
        border-bottom: 1px solid #e0e0e0;
    }
</style>

@code {
    private UserSettings settings = new();
    
    private async Task SaveSettings()
    {
        await UserService.UpdateSettingsAsync(settings);
    }
    
    private class UserSettings
    {
        public bool DarkMode { get; set; }
        public bool EmailNotifications { get; set; }
        public bool TwoFactor { get; set; }
    }
}
```

### Notes

- The **Inline** property is useful for space-constrained layouts where horizontal space is limited
- The **FullWidth** property pairs well with settings panels where you want visual separation between the label and toggle
- Use **OnText** and **OffText** to make the toggle state explicitly clear to users
- The component inherits from **BitInputBase**, so it integrates seamlessly with Blazor's form validation system
- Custom CSS can be applied via **BitToggleClassStyles** for theme matching within your application

---

# Pickers

## DatePicker

### Component Overview

**BitDatePicker** is a Blazor UI component providing a drop-down control optimized for picking a single date from a calendar view. It supports contextual information like the day of the week and allows customization for various selection modes and display options.

### Parameters & Properties

#### Date Selection
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Value` | `TValue?` | `null` | The selected date via two-way binding |
| `MinDate` | `DateTimeOffset?` | `null` | Earliest selectable date |
| `MaxDate` | `DateTimeOffset?` | `null` | Latest selectable date |
| `StartingValue` | `DateTimeOffset?` | `null` | Initial display date when picker is unopened |
| `Mode` | `BitDatePickerMode` | `DatePicker` | Selection mode: `DatePicker` or `MonthPicker` |

#### Display & Formatting
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `DateFormat` | `string?` | `null` | Custom date format string |
| `TimeFormat` | `BitTimeFormat` | `TwentyFourHours` | Time display format: `TwentyFourHours` or `TwelveHours` |
| `Culture` | `CultureInfo` | `CurrentUICulture` | Localization settings for date display |
| `TimeZone` | `TimeZoneInfo?` | `null` | Timezone for date interpretation |
| `Label` | `string?` | `null` | Field label text |
| `Placeholder` | `string` | `empty` | Input placeholder text |

#### Interaction Features
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AllowTextInput` | `bool` | `false` | Enable manual date entry in input field |
| `AutoClose` | `bool` | `true` | Automatically close picker after selection |
| `ReadOnly` | `bool` | `false` | Prevent editing (display only) |
| `Standalone` | `bool` | `false` | Render without input wrapper |
| `IsOpen` | `bool` | `false` | Control picker dropdown visibility |
| `ShowTimePicker` | `bool` | `false` | Include time selection interface |

#### Visual Options
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowWeekNumbers` | `bool` | `false` | Display week numbers in calendar |
| `ShowGoToToday` | `bool` | `true` | Show "Go to Today" navigation button |
| `ShowClearButton` | `bool` | `false` | Display clear button when value exists |
| `ShowCloseButton` | `bool` | `false` | Display close button |
| `HasBorder` | `bool` | `false` | Apply border styling to input |
| `Underlined` | `bool` | `false` | Underline text field instead of border |
| `IconName` | `string` | `CalendarMirrored` | Calendar icon identifier |
| `IconLocation` | `BitIconLocation` | `Right` | Icon placement: `Left` or `Right` |

#### Month Picker Options
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsMonthPickerVisible` | `bool` | `true` | Show month selection in picker |
| `HighlightCurrentMonth` | `bool` | `false` | Highlight the current month |
| `HighlightSelectedMonth` | `bool` | `false` | Highlight the chosen month |
| `ShowMonthPickerAsOverlay` | `bool` | `false` | Display month picker as overlay vs. integrated |

#### Time Control
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `HourStep` | `int` | `1` | Hour increment value for time selection |
| `MinuteStep` | `int` | `1` | Minute increment value for time selection |

#### Validation & Structure
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Required` | `bool` | `false` | Make field mandatory |
| `NoValidate` | `bool` | `false` | Disable validation |
| `InvalidErrorMessage` | `string?` | `null` | Custom validation error text |
| `DisplayName` | `string?` | `null` | Accessible field name for screen readers |
| `Name` | `string?` | `null` | HTML element name attribute |

#### Accessibility & Styling
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaLabel` | `string?` | `null` | Screen reader label for input |
| `CalloutAriaLabel` | `string` | `Calendar` | Screen reader label for calendar popup |
| `Classes` | `BitDatePickerClassStyles` | `null` | Custom CSS classes (60+ properties) |
| `Styles` | `BitDatePickerClassStyles` | `null` | Custom CSS inline styles |
| `Class` | `string?` | `null` | CSS class for root element |
| `Style` | `string?` | `null` | CSS inline styles for root element |
| `Dir` | `BitDir?` | `null` | Text direction: `Ltr`, `Rtl`, or `Auto` |
| `Responsive` | `bool` | `false` | Enable responsive layout |

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<TValue?>` | Fired when selected date value changes |
| `OnClick` | `EventCallback` | Fired when input field is clicked |
| `OnFocus` | `EventCallback` | Fired when input receives focus |
| `OnFocusIn` | `EventCallback` | Fired when focus enters the component |
| `OnFocusOut` | `EventCallback` | Fired when focus leaves the component |

### Templates for Customization

- **`LabelTemplate`** (`RenderFragment?`) - Custom label markup
- **`IconTemplate`** (`RenderFragment?`) - Custom calendar icon markup
- **`DayCellTemplate`** (`RenderFragment<DateTimeOffset>?`) - Custom day cell rendering
- **`MonthCellTemplate`** (`RenderFragment<DateTimeOffset>?`) - Custom month cell rendering
- **`YearCellTemplate`** (`RenderFragment<int>?`) - Custom year cell rendering

### Public Methods & Properties

| Member | Type | Description |
|--------|------|-------------|
| `FocusAsync()` | `ValueTask` | Give input field focus |
| `FocusAsync(bool preventScroll)` | `ValueTask` | Focus with optional scroll prevention |
| `InputElement` | `ElementReference` | Reference to underlying input HTML element |
| `RootElement` | `ElementReference` | Reference to root HTML element |
| `UniqueId` | `Guid` | Unique component identifier |

### Enumerations

**BitDatePickerMode**
- `DatePicker` (0) - Select individual dates
- `MonthPicker` (1) - Select months

**BitTimeFormat**
- `TwentyFourHours` (0) - 24-hour format (00:00-23:59)
- `TwelveHours` (1) - 12-hour format with AM/PM

**BitIconLocation**
- `Left` (0) - Calendar icon on left side
- `Right` (1) - Calendar icon on right side (default)

**BitDir** (Text Direction)
- `Ltr` (0) - Left-to-right
- `Rtl` (1) - Right-to-left
- `Auto` (2) - Automatic detection

### Common Usage Examples

#### Basic Date Picker
```csharp
<BitDatePicker @bind-Value="selectedDate" Label="Select a date" />

@code {
    private DateTimeOffset? selectedDate;
}
```

#### With Min/Max Dates
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Label="Choose a date" 
               MinDate="DateTimeOffset.Now" 
               MaxDate="DateTimeOffset.Now.AddMonths(3)" />
```

#### With Text Input Enabled
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Label="Date (manual entry allowed)" 
               AllowTextInput="true" 
               DateFormat="yyyy-MM-dd" />
```

#### With Time Selection
```csharp
<BitDatePicker @bind-Value="selectedDateTime" 
               Label="Select date and time" 
               ShowTimePicker="true" 
               TimeFormat="BitTimeFormat.TwelveHours" 
               HourStep="1" 
               MinuteStep="15" />
```

#### Month Picker Mode
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Mode="BitDatePickerMode.MonthPicker" 
               Label="Select a month" 
               IsMonthPickerVisible="true" 
               HighlightCurrentMonth="true" />
```

#### With Event Handling
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               OnChange="HandleDateChanged" 
               Label="Date selection" />

@code {
    private DateTimeOffset? selectedDate;
    
    private async Task HandleDateChanged(DateTimeOffset? newDate)
    {
        if (newDate.HasValue)
        {
            Console.WriteLine($"Date changed to: {newDate.Value:yyyy-MM-dd}");
        }
    }
}
```

#### With Custom Culture & Timezone
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Culture="new CultureInfo(\"fr-FR\")" 
               TimeZone="TimeZoneInfo.FindSystemTimeZoneById(\"Central European Standard Time\")" 
               Label="Date" />
```

#### Standalone Calendar (No Input)
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Standalone="true" 
               ShowGoToToday="true" 
               ShowClearButton="true" />
```

#### With Clear & Close Buttons
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Label="Date" 
               ShowClearButton="true" 
               ShowCloseButton="true" 
               AutoClose="false" />
```

#### With Week Numbers
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Label="Date" 
               ShowWeekNumbers="true" />
```

#### Custom Styling
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Label="Date" 
               HasBorder="true" 
               Class="custom-date-picker" 
               Style="width: 300px;" />
```

#### Validation Example
```csharp
<BitDatePicker @bind-Value="startDate" 
               Label="Start Date" 
               Required="true" 
               InvalidErrorMessage="Start date is required" />

<BitDatePicker @bind-Value="endDate" 
               Label="End Date" 
               MinDate="startDate" 
               Required="true" 
               InvalidErrorMessage="End date must be after start date" />
```

### CSS Customization

BitDatePicker supports extensive CSS customization through the `BitDatePickerClassStyles` property with 60+ style properties including:

- **Root & Input**: Root, Input, Icon, InputWrapper, InputAndIconContainer
- **Calendar**: Overlay, Callout, DayPickerWrapper, DayPickerHeader
- **Day Selection**: DayButton, TodayDayButton, SelectedDayButton, DisabledDayButton
- **Month/Year**: MonthButton, YearButton, SelectedMonthButton, SelectedYearButton
- **Navigation**: PreviousMonthButton, NextMonthButton, PreviousYearButton, NextYearButton
- **Time Picker**: TimePickerContainer, TimePickerHourMinuteSeparator, TimerPickerLabel
- **Other**: WeekNumberCell, MonthPickerWrapper, YearPickerWrapper

Example custom styling:
```csharp
<BitDatePicker @bind-Value="selectedDate" 
               Classes="@(new BitDatePickerClassStyles 
               { 
                   Root = "custom-root-class",
                   Input = "custom-input-class",
                   DayButton = "custom-day-button",
                   SelectedDayButton = "custom-selected-day"
               })" />
```

---

## TimePicker

### Component Overview

**BitTimePicker** is a Blazor UI component that provides a drop-down control optimized for picking a single time from a clock view. It supports both 12-hour (AM/PM) and 24-hour formats with customizable hour/minute increments and flexible styling options.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **Value** | `TimeSpan?` | `null` | The selected time value |
| **TimeFormat** | `BitTimeFormat` | `TwentyFourHours` | Time display format (24H or 12H with AM/PM) |
| **AllowTextInput** | `bool` | `false` | Enables users to type time directly into the input field |
| **HourStep** | `int` | `1` | Hour increment interval in the picker |
| **MinuteStep** | `int` | `1` | Minute increment interval in the picker |
| **Standalone** | `bool` | `false` | Renders only the time picker without input field wrapper |
| **ReadOnly** | `bool` | `false` | Prevents manual editing of the time value |
| **Required** | `bool` | `false` | Marks the field as mandatory for form validation |
| **Placeholder** | `string` | `null` | Placeholder text displayed in the input field |
| **IconName** | `string` | `"Clock"` | Icon identifier to display in the input field |
| **IconLocation** | `BitIconLocation` | `Right` | Position of the icon (Left or Right) |
| **ShowCloseButton** | `bool` | `false` | Displays a close button within the time picker dropdown |
| **ValueFormat** | `string` | `null` | Custom time formatting pattern (e.g., "HH:mm", "hh:mm tt") |
| **Culture** | `CultureInfo` | `null` | Culture info for localization and formatting |
| **Disabled** | `bool` | `false` | Disables the time picker component |
| **Label** | `string` | `null` | Label text displayed above the input |
| **Class** | `string` | `null` | Custom CSS class names |
| **Style** | `string` | `null` | Inline styles |

### Time Format Modes

The **BitTimeFormat** enum provides two primary display modes:

```csharp
public enum BitTimeFormat
{
    TwentyFourHours = 0,  // 24-hour format (00:00 - 23:59)
    TwelveHours = 1       // 12-hour format with AM/PM (12:00 AM - 11:59 PM)
}
```

### Events/Callbacks

| Event | Type | Description |
|-------|------|-------------|
| **OnSelectTime** | `EventCallback<TimeSpan?>` | Triggered when the time value is selected or changed |
| **OnChange** | `EventCallback<string>` | Fires when input value changes |
| **OnClick** | `EventCallback<MouseEventArgs>` | Triggered when the input field is clicked |
| **OnFocus** | `EventCallback<FocusEventArgs>` | Fires when component receives focus |
| **OnFocusIn** | `EventCallback<FocusEventArgs>` | Triggered when component gains focus |
| **OnFocusOut** | `EventCallback<FocusEventArgs>` | Triggered when component loses focus |

### Code Examples

#### Basic Time Picker (24-Hour Format)

```razor
@page "/timepicker-basic"
@using Bit.BlazorUI

<BitTimePicker @bind-Value="selectedTime" 
               Label="Select Time"
               Placeholder="Choose a time" />

<p>Selected time: @selectedTime?.ToString("HH:mm")</p>

@code {
    private TimeSpan? selectedTime;
}
```

#### 12-Hour Format with AM/PM

```razor
<BitTimePicker @bind-Value="appointmentTime"
               TimeFormat="BitTimeFormat.TwelveHours"
               Label="Appointment Time"
               Placeholder="Pick time (12-hour)" />

<p>Time: @appointmentTime?.ToString("hh:mm tt")</p>

@code {
    private TimeSpan? appointmentTime;
}
```

#### Time Picker with Custom Increments

```razor
<BitTimePicker @bind-Value="meetingTime"
               TimeFormat="BitTimeFormat.TwentyFourHours"
               HourStep="1"
               MinuteStep="15"
               Label="Meeting Time"
               AllowTextInput="true" />

<p>Meeting scheduled for: @meetingTime?.ToString("HH:mm")</p>

@code {
    private TimeSpan? meetingTime;
}
```

#### Read-Only Time Picker

```razor
<BitTimePicker Value="new TimeSpan(14, 30, 0)"
               ReadOnly="true"
               Label="System Start Time"
               TimeFormat="BitTimeFormat.TwentyFourHours" />

@code {
}
```

#### Time Picker with Event Handling

```razor
<BitTimePicker @bind-Value="clockTime"
               OnSelectTime="HandleTimeSelect"
               Label="Clock Time"
               TimeFormat="BitTimeFormat.TwentyFourHours" />

<div>
    @if (!string.IsNullOrEmpty(message))
    {
        <p style="color: green;">@message</p>
    }
</div>

@code {
    private TimeSpan? clockTime;
    private string message = string.Empty;

    private Task HandleTimeSelect(TimeSpan? time)
    {
        message = time.HasValue 
            ? $"Time selected: {time.Value:HH:mm:ss}"
            : "Time cleared";
        return Task.CompletedTask;
    }
}
```

#### Standalone Time Picker (No Input Field)

```razor
<BitTimePicker @bind-Value="standaloneTime"
               Standalone="true"
               TimeFormat="BitTimeFormat.TwentyFourHours" />

<p>Selected: @standaloneTime?.ToString("HH:mm")</p>

@code {
    private TimeSpan? standaloneTime;
}
```

#### Time Picker with Text Input Support

```razor
<BitTimePicker @bind-Value="flexibleTime"
               AllowTextInput="true"
               TimeFormat="BitTimeFormat.TwelveHours"
               Label="Flexible Time Input"
               Placeholder="Type or select time (e.g., 02:30 PM)" />

<p>Entered time: @flexibleTime?.ToString("hh:mm tt")</p>

@code {
    private TimeSpan? flexibleTime;
}
```

#### Required Field Validation

```razor
<EditForm Model="@scheduleModel" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    
    <BitTimePicker @bind-Value="scheduleModel.StartTime"
                   TimeFormat="BitTimeFormat.TwentyFourHours"
                   Required="true"
                   Label="Event Start Time"
                   Placeholder="Required" />
    
    <ValidationMessage For="@(() => scheduleModel.StartTime)" />
    
    <button type="submit" class="btn btn-primary">Schedule Event</button>
</EditForm>

@code {
    private ScheduleModel scheduleModel = new();

    private class ScheduleModel
    {
        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan? StartTime { get; set; }
    }

    private async Task HandleSubmit()
    {
        // Process the scheduled event
        await Task.CompletedTask;
    }
}
```

#### Icon Customization

```razor
<BitTimePicker @bind-Value="time1"
               Label="Default Icon (Right)"
               IconName="Clock"
               IconLocation="BitIconLocation.Right" />

<BitTimePicker @bind-Value="time2"
               Label="Icon on Left"
               IconName="Clock"
               IconLocation="BitIconLocation.Left" />

<BitTimePicker @bind-Value="time3"
               Label="Custom Close Button"
               ShowCloseButton="true" />

@code {
    private TimeSpan? time1, time2, time3;
}
```

#### Formatted Time Picker with Culture Support

```razor
@using System.Globalization

<BitTimePicker @bind-Value="localizedTime"
               TimeFormat="BitTimeFormat.TwelveHours"
               Culture="new CultureInfo("fr-FR")"
               ValueFormat="HH:mm"
               Label="Heure (French Localization)" />

<p>Time: @localizedTime?.ToString("hh:mm tt")</p>

@code {
    private TimeSpan? localizedTime;
}
```

### Key Features

- **Multiple Time Formats**: Switch between 24-hour and 12-hour (AM/PM) displays
- **Flexible Input**: Support for clicking/selecting time or typing directly
- **Custom Increments**: Configure hour and minute step intervals (e.g., 15-minute slots)
- **Form Integration**: Works seamlessly with `EditForm` for validation
- **Icon Support**: Customizable icons with placement options
- **Responsive Design**: Adapts to different screen sizes
- **RTL Support**: Compatible with right-to-left languages
- **Accessibility**: Proper ARIA labels and keyboard navigation

### Best Practices

1. **Use `AllowTextInput`** when users need fast time entry alongside picker convenience
2. **Set appropriate `HourStep`/`MinuteStep`** based on your use case (e.g., 15 minutes for appointments)
3. **Leverage `ReadOnly`** for displaying system-assigned times that shouldn't be changed
4. **Apply `Required`** for mandatory time selection fields within forms
5. **Use `OnSelectTime`** callback for real-time validation or dependent field updates
6. **Consider timezone implications** when storing/retrieving `TimeSpan` values

Sources:
- [Bit.BlazorUI TimePicker Documentation](https://blazorui.bitplatform.dev/components/timepicker)

---

## DateRangePicker

**Component Name:** `BitDateRangePicker`

**Description:** A drop-down control optimized for picking two dates from a calendar view where contextual information like the day of the week or fullness of the calendar is important.

### Core Parameters

#### Date & Time Selection
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `StartingValue` | `BitDateRangePickerValue?` | - | Sets initial date/time when opened without selection |
| `MinDate` | `DateTimeOffset?` | - | Minimum selectable date |
| `MaxDate` | `DateTimeOffset?` | - | Maximum selectable date |
| `MaxRange` | `TimeSpan?` | - | Maximum allowed range between start and end dates |
| `TimeZone` | `TimeZoneInfo?` | - | Timezone for interpreting dates |
| `ShowTimePicker` | `bool` | `false` | Enables time selection alongside dates |
| `TimeFormat` | `BitTimeFormat` | - | 24-hour or 12-hour format |
| `HourStep` | `int` | - | Hour increment/decrement step |
| `MinuteStep` | `int` | - | Minute increment/decrement step |

#### Display & Behavior
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AutoClose` | `bool` | `true` | Closes automatically after selecting second date |
| `AllowTextInput` | `bool` | `false` | Permits manual date string entry |
| `ReadOnly` | `bool` | `false` | Prevents manual editing |
| `Placeholder` | `string?` | - | Input placeholder text |
| `DateFormat` | `string?` | - | Custom date formatting pattern |
| `ValueFormat` | `string?` | - | Display format for selected range |
| `Standalone` | `bool` | `false` | Renders independently without input wrapper |
| `HasBorder` | `bool` | `true` | Includes border styling |
| `Underlined` | `bool` | `false` | Underlined text field style |

#### Visibility Controls
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowWeekNumbers` | `bool` | `false` | Displays week numbers 1-53 |
| `ShowCloseButton` | `bool` | `false` | Displays close button |
| `ShowClearButton` | `bool` | `false` | Shows clear button when value exists |
| `ShowGoToToday` | `bool` | `true` | Displays "go to today" button |
| `IsMonthPickerVisible` | `bool` | `true` | Month picker visibility |
| `ShowMonthPickerAsOverlay` | `bool` | `false` | Overlay vs. integrated month picker |
| `ShowTimePickerAsOverlay` | `bool` | `false` | Overlay vs. integrated time picker |
| `IsOpen` | `bool` | `false` | Controls callout open state |
| `Responsive` | `bool` | `false` | Enables responsive design for small screens |

#### Visual Enhancements
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `HighlightCurrentMonth` | `bool` | `false` | Highlights current month in picker |
| `HighlightSelectedMonth` | `bool` | `false` | Highlights selected month |
| `IconName` | `string` | `"CalendarMirrored"` | Calendar icon identifier |
| `IconLocation` | `BitIconLocation` | `Right` | Icon position (Left or Right) |
| `IconTemplate` | `RenderFragment?` | - | Custom icon rendering |

### Date Range Value Handling

The component uses the `BitDateRangePickerValue` type to manage the selected date range:

```csharp
public class BitDateRangePickerValue
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}
```

**Usage:**
```razor
@page "/daterangepicker-example"

<BitDateRangePicker @bind-Value="selectedRange" />

@code {
    private BitDateRangePickerValue? selectedRange;
}
```

### Events & Callbacks

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<BitDateRangePickerValue?>` | Fires when selected range changes |
| `OnClick` | `EventCallback` | Triggers on input click |
| `OnFocus` | `EventCallback` | Triggers on input focus |
| `OnFocusIn` | `EventCallback` | Fires when focus enters input |
| `OnFocusOut` | `EventCallback` | Fires when focus leaves input |

### Template Support

| Template | Type | Description |
|----------|------|-------------|
| `LabelTemplate` | `RenderFragment?` | Custom label rendering |
| `DayCellTemplate` | `RenderFragment<DateTimeOffset>?` | Customize individual day cells |
| `MonthCellTemplate` | `RenderFragment<DateTimeOffset>?` | Customize month cells |
| `YearCellTemplate` | `RenderFragment<int>?` | Customize year cells |

### Accessibility & Localization

| Parameter | Type | Description |
|-----------|------|-------------|
| `Culture` | `CultureInfo` | Defaults to CurrentUICulture; supports custom cultures including Farsi (fa-IR) |
| `AriaLabel` | `string?` | Screen reader accessible label |
| `Label` | `string?` | Component label text |
| `DisplayName` | `string?` | Field display name for validation |
| `Required` | `bool` | Marks input as required |
| `Dir` | `BitDir` | Text directionality (Ltr, Rtl, Auto) |

### Code Examples

#### Basic Range Selection
```razor
@page "/daterangepicker-basic"

<BitDateRangePicker 
    @bind-Value="selectedRange"
    Placeholder="Select date range"
    Label="Date Range" />

<div class="mt-3">
    @if (selectedRange?.StartDate.HasValue ?? false)
    {
        <p>Start Date: @selectedRange.StartDate.Value.ToString("yyyy-MM-dd")</p>
        <p>End Date: @selectedRange.EndDate?.ToString("yyyy-MM-dd")</p>
    }
</div>

@code {
    private BitDateRangePickerValue? selectedRange;
}
```

#### With Time Selection
```razor
<BitDateRangePicker 
    @bind-Value="selectedRange"
    ShowTimePicker="true"
    TimeFormat="BitTimeFormat.TwentyFourHour"
    HourStep="1"
    MinuteStep="15"
    Label="Date & Time Range" />
```

#### With Date Constraints
```razor
<BitDateRangePicker 
    @bind-Value="selectedRange"
    MinDate="DateTimeOffset.Now.AddDays(-30)"
    MaxDate="DateTimeOffset.Now.AddDays(30)"
    MaxRange="TimeSpan.FromDays(14)"
    Label="Select within 2 weeks (last 30-30 days)" />
```

#### With Change Handling
```razor
<BitDateRangePicker 
    Value="selectedRange"
    ValueChanged="@OnDateRangeChanged"
    Label="Booking Dates"
    OnChange="@OnChange" />

@code {
    private BitDateRangePickerValue? selectedRange;

    private async Task OnDateRangeChanged(BitDateRangePickerValue? value)
    {
        selectedRange = value;
        if (selectedRange?.StartDate.HasValue ?? false)
        {
            // Perform action when range is selected
            await RefreshData(selectedRange.StartDate.Value, selectedRange.EndDate);
        }
    }

    private async Task OnChange(BitDateRangePickerValue? value)
    {
        // Handle change event
        await Task.CompletedTask;
    }
}
```

#### Read-Only Display
```razor
<BitDateRangePicker 
    Value="selectedRange"
    ReadOnly="true"
    Label="Selected Booking Period" />
```

#### Responsive with Custom Styling
```razor
<BitDateRangePicker 
    @bind-Value="selectedRange"
    Responsive="true"
    ShowWeekNumbers="true"
    ShowGoToToday="true"
    ShowClearButton="true"
    IconLocation="BitIconLocation.Left"
    Label="Report Period" />
```

### Styling & Customization

The component supports `BitDateRangePickerClassStyles` which enables customization of 60+ CSS elements including:
- Root container
- Input field
- Callout/dropdown
- Day picker
- Month picker
- Year picker
- Time picker
- Individual buttons and icons

### Key Use Cases

- **Booking Systems:** Hotel reservations, event scheduling, rental periods
- **Reporting:** Custom date range filters for analytics dashboards
- **Data Filtering:** Advanced search with temporal constraints
- **Time-Aware Applications:** When selecting ranges with specific times (appointments, logs, events)

---

This documentation provides complete coverage of the BitDateRangePicker component with practical examples ready for implementation in your Blazor Server application.

---

## CircularTimePicker

The **CircularTimePicker** is a drop-down control optimized for picking a single time from an interactive clock view in Bit.BlazorUI. It provides an intuitive circular interface with both 12-hour and 24-hour format support, making it ideal for user-friendly time selection across responsive interfaces.

### Component Description

BitCircularTimePicker offers a visual clock-based approach to time selection, reducing input errors and improving user experience compared to traditional text-based time inputs. The component handles both direct text input and interactive clock selection with full localization support.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AllowTextInput` | `bool` | `false` | Permits users to directly input time as text strings instead of only using the clock interface |
| `TimeFormat` | `BitTimeFormat` | `TwentyFourHours` | Controls clock display format: `TwentyFourHours` (00-23) or `TwelveHours` (1-12 with AM/PM) |
| `ValueFormat` | `string?` | `null` | Custom time formatting pattern (e.g., "HH:mm", "hh:mm tt") for value parsing and display |
| `Placeholder` | `string?` | `null` | Text displayed when no time is selected |
| `Label` | `string?` | `null` | Text label displayed above or beside the input field |
| `IsOpen` | `bool` | `false` | Controls whether the clock picker dropdown is visible |
| `ReadOnly` | `bool` | `false` | Prevents manual time editing and user interaction |
| `Required` | `bool` | `false` | Marks the field as mandatory in form validation |
| `Responsive` | `bool` | `false` | Enables mobile and tablet adaptation for the clock interface |
| `Culture` | `CultureInfo` | `CurrentUICulture` | Sets localization for time display, AM/PM labels, and formatting |

### Clock Face UI Options

**24-Hour Format:**
- Numbered positions display hours 00-23 in circular arrangement
- Direct hour selection from full day range
- Ideal for applications requiring 24-hour time reference

**12-Hour Format:**
- Numbered positions display hours 1-12
- AM/PM toggle buttons for period selection
- Rotating pointer mechanism for visual feedback
- Standard business/consumer application format

**Interactive Elements:**
- Selectable hour/minute buttons on clock face
- Rotating pointer to indicate current selection
- Touch-friendly sizing on mobile devices (when Responsive=true)

### Events & Callbacks

| Event | Trigger | Use Case |
|-------|---------|----------|
| `OnSelectTime` | When user selects time from clock | Primary handler for time selection logic |
| `OnChange` | When selected time value changes | Standard value change notification |
| `OnClick` | When input field is clicked | Pre-selection interaction handling |
| `OnFocus` / `OnFocusIn` | When component gains focus | Initialize time picker or prepare UI |
| `OnFocusOut` | When component loses focus | Validate time or trigger secondary actions |

### Code Examples

#### Basic Time Picker (24-Hour Format)

```razor
@page "/time-picker"
@using Bit.BlazorUI

<BitCircularTimePicker 
    @bind-Value="selectedTime"
    Label="Select Meeting Time"
    TimeFormat="BitTimeFormat.TwentyFourHours"
    Placeholder="HH:mm" />

<p>Selected time: @selectedTime?.ToString("HH:mm")</p>

@code {
    private TimeSpan? selectedTime;
}
```

#### 12-Hour Format with Text Input

```razor
<BitCircularTimePicker 
    @bind-Value="appointmentTime"
    Label="Appointment Time"
    TimeFormat="BitTimeFormat.TwelveHours"
    AllowTextInput="true"
    Placeholder="hh:mm tt"
    ValueFormat="hh:mm tt" />

<p>Appointment: @appointmentTime?.ToString("hh:mm tt")</p>

@code {
    private TimeSpan? appointmentTime;
}
```

#### Required Field with Validation

```razor
<EditForm Model="@scheduleModel" OnValidSubmit="@HandleSubmit">
    <DataAnnotationsValidator />
    
    <BitCircularTimePicker 
        @bind-Value="scheduleModel.StartTime"
        Label="Start Time"
        Required="true"
        TimeFormat="BitTimeFormat.TwentyFourHours" />
    
    <ValidationMessage For="@(() => scheduleModel.StartTime)" />
    
    <button type="submit">Schedule</button>
</EditForm>

@code {
    private ScheduleModel scheduleModel = new();

    private async Task HandleSubmit()
    {
        // Process valid schedule
        await Task.Delay(100);
    }

    public class ScheduleModel
    {
        [Required(ErrorMessage = "Start time is required")]
        public TimeSpan? StartTime { get; set; }
    }
}
```

#### Event Handling Example

```razor
<BitCircularTimePicker 
    @bind-Value="shiftTime"
    Label="Work Shift Time"
    OnSelectTime="@OnTimeSelected"
    OnFocusOut="@OnTimeLostFocus" />

<div>
    @if (!string.IsNullOrEmpty(message))
    {
        <p>@message</p>
    }
</div>

@code {
    private TimeSpan? shiftTime;
    private string message = "";

    private Task OnTimeSelected()
    {
        message = $"Shift time set to: {shiftTime:HH:mm}";
        return Task.CompletedTask;
    }

    private Task OnTimeLostFocus()
    {
        if (shiftTime.HasValue)
            message = $"Confirmed time: {shiftTime:HH:mm}";
        return Task.CompletedTask;
    }
}
```

#### Mobile-Responsive Time Picker

```razor
<BitCircularTimePicker 
    @bind-Value="eventTime"
    Label="Event Time"
    TimeFormat="BitTimeFormat.TwelveHours"
    Responsive="true"
    AllowTextInput="true"
    Culture="@CultureInfo.GetCultureInfo("en-US")" />

<p>Event scheduled for: @eventTime?.ToString("hh:mm tt", CultureInfo.CurrentCulture)</p>

@code {
    private TimeSpan? eventTime;
}
```

#### Read-Only Display

```razor
<BitCircularTimePicker 
    Value="new TimeSpan(14, 30, 0)"
    Label="Server Time"
    ReadOnly="true"
    TimeFormat="BitTimeFormat.TwentyFourHours"
    ValueFormat="HH:mm" />

@code {
    // Display only - cannot be modified by user
}
```

### Styling & Customization

The BitCircularTimePicker supports granular CSS customization through `BitCircularTimePickerClassStyles` with properties for:
- Root container elements
- Clock display components
- Hour/minute buttons
- Selection containers
- Icon styling

### Best Practices

1. **Format Selection**: Use 24-hour format for technical applications (scheduling systems, APIs) and 12-hour for consumer-facing interfaces
2. **Text Input**: Enable `AllowTextInput` for power users but keep validation strict
3. **Responsiveness**: Always enable `Responsive="true"` for mobile applications
4. **Validation**: Use `Required="true"` with EditForm for mandatory time fields
5. **Localization**: Set `Culture` appropriately for international applications
6. **Event Handling**: Use `OnSelectTime` for immediate feedback and `OnChange` for persistence

---

**Documentation Source:** [Bit.BlazorUI CircularTimePicker](https://blazorui.bitplatform.dev/components/circulartimepicker)

---

## ColorPicker

The **BitColorPicker** component is used to browse through and select colors, enabling users to navigate through a color spectrum or specify colors using RGB or hexadecimal values. It supports automatic color format detection and includes optional alpha transparency control.

### Component Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Color` | `string` | - | CSS-compatible color value (hex or RGB format) |
| `Alpha` | `double` | 1 | Transparency level of the color (0-1) |
| `ShowAlphaSlider` | `bool` | `false` | Enables alpha value editing slider for transparency control |
| `ShowPreview` | `bool` | `false` | Displays color preview box |
| `Class` | `string` | - | CSS class names for styling |
| `Style` | `string` | - | Inline CSS styling |
| `Id` | `string` | - | Unique identifier |
| `IsEnabled` | `bool` | `true` | Enable/disable user interaction |
| `AriaLabel` | `string` | - | Accessibility label for screen readers |
| `TabIndex` | `int` | - | Keyboard navigation order |
| `Dir` | `Direction` | `Auto` | Text directionality (Ltr, Rtl, Auto) |
| `Visibility` | `Visibility` | `Visible` | Display state (Visible, Hidden, Collapsed) |

### Supported Color Formats

The ColorPicker automatically detects and supports the following color formats:

- **Hexadecimal**: `#FFFFFF`, `#000000`
- **RGB**: `rgb(255, 255, 255)`, `rgb(0, 0, 0)`
- **RGBA**: `rgba(255, 255, 255, 1)` (when alpha is used)
- **HSV**: Available through public API for programmatic access

### Events

#### OnChange
Fires when the user selects a new color. The callback provides:
- **Color**: Selected color in the format matching the initial input (hex or RGB)
- **Alpha**: Current alpha/transparency value (0-1)

### Code Examples

#### Basic Color Selection
```razor
<BitColorPicker @bind-Color="selectedColor" />

@code {
    private string selectedColor = "#FF5733";
}
```

#### With Alpha Transparency
```razor
<BitColorPicker 
    @bind-Color="selectedColor"
    @bind-Alpha="alphaValue"
    ShowAlphaSlider="true" />

@code {
    private string selectedColor = "rgb(255, 87, 51)";
    private double alphaValue = 0.8;
}
```

#### With Preview and Change Handler
```razor
<BitColorPicker 
    Color="@selectedColor"
    ShowPreview="true"
    OnChange="@HandleColorChange"
    ShowAlphaSlider="true" />

<p>Selected Color: @selectedColor</p>

@code {
    private string selectedColor = "#3498DB";

    private async Task HandleColorChange(BitColorPickerChangeEventArgs args)
    {
        selectedColor = args.Color;
        await InvokeAsync(StateHasChanged);
    }
}
```

#### Styled ColorPicker with Custom CSS
```razor
<BitColorPicker 
    @bind-Color="brandColor"
    Class="custom-color-picker"
    Style="width: 100%; margin: 1rem 0;"
    AriaLabel="Brand Color Selection"
    ShowPreview="true" />

<style>
    .custom-color-picker {
        border: 2px solid #ddd;
        border-radius: 8px;
        padding: 1rem;
    }
</style>

@code {
    private string brandColor = "#0078D4";
}
```

#### Disabled State
```razor
<BitColorPicker 
    Color="@lockedColor"
    IsEnabled="false"
    ShowPreview="true"
    AriaLabel="Read-only color display" />

@code {
    private string lockedColor = "#2ECC71";
}
```

### Best Practices

1. **Format Consistency**: Initialize with either hex or RGB format; the component maintains that format in output
2. **Alpha Channel**: Use `ShowAlphaSlider="true"` only when transparency is needed
3. **Preview**: Enable `ShowPreview="true"` for better UX when precision color selection is important
4. **Accessibility**: Always provide an `AriaLabel` for screen reader users
5. **Binding**: Use `@bind-Color` and `@bind-Alpha` for automatic two-way data binding

Sources:
- [Bit.BlazorUI ColorPicker Component](https://blazorui.bitplatform.dev/components/colorpicker)

---

# Layouts

## Footer

### Description
The BitFooter component displays a colored bar (with text and possibly other components) at the bottom of a site or application. It supports CSS styling and can be positioned fixed at the page bottom.

### Parameters

#### BitFooter-Specific Parameters
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `ChildContent` | `RenderFragment?` | `null` | Content rendered inside the footer |
| `Height` | `int?` | `null` | Footer height in pixels |
| `Fixed` | `bool` | `false` | Fixed positioning at page bottom |

#### Inherited Parameters (BitComponentBase)
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `AriaLabel` | `string?` | `null` | Accessible label for assistive technologies |
| `Class` | `string?` | `null` | CSS class names |
| `Dir` | `BitDir?` | `null` | Text directionality (Ltr/Rtl/Auto) |
| `HtmlAttributes` | `Dictionary<string, object>` | `new Dictionary` | Additional HTML attributes |
| `Id` | `string?` | `null` | Unique element identifier |
| `IsEnabled` | `bool` | `true` | Component interaction state |
| `Style` | `string?` | `null` | CSS style string |
| `TabIndex` | `string?` | `null` | Keyboard navigation order |
| `Visibility` | `BitVisibility` | `Visible` | Visibility state (Visible/Hidden/Collapsed) |

### Public Members
- **UniqueId** (`Guid`): Auto-assigned unique identifier for the component instance
- **RootElement** (`ElementReference`): Reference to the root HTML element

### Code Examples

#### Basic Footer
```csharp
<BitFooter>I'm a Footer</BitFooter>
```

#### Fixed Footer with Height
```csharp
<BitFooter Fixed="true" Height="80">
    <div style="padding: 20px;">
        <p>&copy; 2024 Your Company. All rights reserved.</p>
    </div>
</BitFooter>
```

#### Advanced Footer with Links and Content
```csharp
<BitFooter Class="footer-branded" Style="background-color: #333; color: white;">
    <div style="padding: 20px; display: flex; justify-content: space-between;">
        <div>
            <h4>About</h4>
            <ul>
                <li><a href="/about">About Us</a></li>
                <li><a href="/careers">Careers</a></li>
            </ul>
        </div>
        <div>
            <h4>Contact</h4>
            <p>Email: info@example.com</p>
            <p>Phone: (555) 123-4567</p>
        </div>
        <div>
            <h4>Follow Us</h4>
            <ul>
                <li><a href="#">Twitter</a></li>
                <li><a href="#">LinkedIn</a></li>
            </ul>
        </div>
    </div>
</BitFooter>
```

### Events & Callbacks
No specific events or callbacks are documented for the BitFooter component. Styling and content updates are handled through standard parameter bindings and CSS approaches.

### Notes
- The component inherits from `BitComponentBase`, providing standard component functionality
- Styling uses standard CSS approaches via the `Style` and `Class` parameters
- The `Fixed` parameter enables sticky footer behavior at page bottom
- The component supports full HTML content through the `ChildContent` render fragment, allowing flexible layouts

---

## Grid

The **BitGrid** is a flexible and customizable grid layout component offering responsive columns and alignment flexibility for structured content presentation. It provides a 12-column layout system by default with full support for responsive breakpoints and customizable spacing.

### Component Description

BitGrid enables developers to create responsive grid-based layouts with easy column configuration, spacing control, and horizontal alignment options. The component supports both grid-level and item-level responsiveness through dedicated breakpoint properties.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | RenderFragment? | - | Grid content container |
| `Columns` | int | 12 | Defines the total number of grid columns |
| `Span` | int | 1 | Defines individual item column span width |
| `HorizontalAlign` | BitAlignment | Start | Child alignment direction (Start, End, Center, SpaceBetween, SpaceAround, SpaceEvenly, Baseline, Stretch) |
| `Spacing` | string | 4px | Gap between grid items (applies to both horizontal and vertical) |
| `HorizontalSpacing` | string? | - | Horizontal gap override between grid items |
| `VerticalSpacing` | string? | - | Vertical gap override between grid items |
| `Class` | string? | - | CSS class names |
| `Style` | string? | - | Inline CSS styles |
| `Id` | string? | - | HTML element ID |
| `AriaLabel` | string? | - | ARIA label for accessibility |
| `Dir` | string? | - | Text direction (LTR/RTL) |
| `IsEnabled` | bool | true | Enable/disable the grid |
| `TabIndex` | int? | - | Tab index for keyboard navigation |
| `Visibility` | string? | - | Visibility control |

### Responsive Breakpoints

BitGrid supports six responsive breakpoints for dynamic column configuration at the grid level:

| Breakpoint | Property | Description |
|------------|----------|-------------|
| Extra Small | `ColumnsXs` | Extra small screens |
| Small | `ColumnsSm` | Small screens |
| Medium | `ColumnsMd` | Medium screens |
| Large | `ColumnsLg` | Large screens |
| Extra Large | `ColumnsXl` | Extra large screens |
| Extra Extra Large | `ColumnsXxl` | Extra extra large screens |

Individual grid items also support item-level breakpoint properties (`Xs`, `Sm`, `Md`, `Lg`, `Xl`, `Xxl`) to specify column spans at different screen sizes.

### BitGridItem Properties

Grid items (`BitGridItem`) contained within a BitGrid support the following properties:

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ChildContent` | RenderFragment? | - | Item content |
| `ColumnSpan` | int | 1 | Column width (number of columns to occupy) |
| `Xs` | int? | - | Column span for extra small breakpoint |
| `Sm` | int? | - | Column span for small breakpoint |
| `Md` | int? | - | Column span for medium breakpoint |
| `Lg` | int? | - | Column span for large breakpoint |
| `Xl` | int? | - | Column span for extra large breakpoint |
| `Xxl` | int? | - | Column span for extra extra large breakpoint |

### Alignment Options

The `HorizontalAlign` parameter accepts values from the `BitAlignment` enum:

- **Start**: Items aligned to the start
- **End**: Items aligned to the end
- **Center**: Items centered
- **SpaceBetween**: Equal space between items
- **SpaceAround**: Equal space around items
- **SpaceEvenly**: Equal space between and around items
- **Baseline**: Items aligned to baseline
- **Stretch**: Items stretched to fill available space

### Spacing Configuration

Control gaps between grid items using:

```
Spacing="4px"              <!-- Default gap (horizontal and vertical) -->
HorizontalSpacing="0.5rem" <!-- Customize left/right gaps -->
VerticalSpacing="1rem"     <!-- Customize top/bottom gaps -->
```

### Column Span Configuration

Configure how many columns an item occupies:

```csharp
<BitGridItem ColumnSpan="4">  <!-- Occupies 4 of 12 columns -->
    <!-- Item content -->
</BitGridItem>

<BitGridItem ColumnSpan="6">  <!-- Occupies 6 of 12 columns -->
    <!-- Item content -->
</BitGridItem>

<BitGridItem ColumnSpan="2">  <!-- Occupies 2 of 12 columns -->
    <!-- Item content -->
</BitGridItem>
```

### Basic Grid Setup

A standard 12-column grid with equal spacing and default alignment:

```html
<BitGrid Columns="12" Spacing="4px" HorizontalAlign="BitAlignment.Start">
    <BitGridItem ColumnSpan="4">
        <!-- Content for column 1-4 -->
    </BitGridItem>
    <BitGridItem ColumnSpan="4">
        <!-- Content for column 5-8 -->
    </BitGridItem>
    <BitGridItem ColumnSpan="4">
        <!-- Content for column 9-12 -->
    </BitGridItem>
</BitGrid>
```

### Responsive Grid Example

Grid with responsive column configuration across different breakpoints:

```html
<BitGrid 
    ColumnsXs="1" 
    ColumnsSm="2" 
    ColumnsMd="3" 
    ColumnsLg="4" 
    ColumnsXl="6" 
    ColumnsXxl="12"
    Spacing="1rem">
    
    <BitGridItem Xs="1" Sm="2" Md="3" Lg="4">
        <!-- Item content -->
    </BitGridItem>
    
    <BitGridItem Xs="1" Sm="2" Md="3" Lg="4">
        <!-- Item content -->
    </BitGridItem>
</BitGrid>
```

### Spacing and Alignment Example

Customize spacing and alignment for precise layout control:

```html
<BitGrid 
    Columns="12" 
    HorizontalSpacing="0.5rem"
    VerticalSpacing="1rem"
    HorizontalAlign="BitAlignment.SpaceBetween">
    
    <BitGridItem ColumnSpan="5">
        <!-- Content -->
    </BitGridItem>
    
    <BitGridItem ColumnSpan="5">
        <!-- Content -->
    </BitGridItem>
</BitGrid>
```

### Usage Notes

- The default grid uses a **12-column layout system**, providing flexibility for common responsive design patterns (4 equal columns, 3 equal columns, 6 equal columns, etc.)
- **Responsive breakpoints** can be configured at both the grid container level and individual item level for maximum flexibility
- **Spacing values** can use any CSS unit (px, rem, em, etc.)
- **Alignment options** control how child items are distributed horizontally within the grid
- The component inherits from `BitComponentBase`, providing standard HTML attributes and accessibility features

---

## Header

### Description
The BitHeader component displays a title and optional additional components in a colored bar at the top of a site or application, using the current primary background color.

### Parameters

#### BitHeader-Specific Parameters

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Content to be rendered inside the BitHeader |
| `Height` | `int?` | `null` | Header height in pixels |
| `Fixed` | `bool` | `false` | Renders header with fixed positioning at page top |

#### Inherited Parameters (BitComponentBase)

| Name | Type | Default | Description |
|------|------|---------|-------------|
| `AriaLabel` | `string?` | `null` | Accessible label for assistive technologies |
| `Class` | `string?` | `null` | CSS class name(s) to apply |
| `Dir` | `BitDir?` | `null` | Text directionality (Ltr, Rtl, Auto) |
| `HtmlAttributes` | `Dictionary` | empty | Additional HTML attributes |
| `Id` | `string?` | `null` | Unique identifier for root element |
| `IsEnabled` | `bool` | `true` | Component interactivity state |
| `Style` | `string?` | `null` | CSS style string to apply |
| `TabIndex` | `string?` | `null` | Keyboard navigation tab order |
| `Visibility` | `BitVisibility` | `Visible` | Visibility state (Visible, Hidden, Collapsed) |

### Public Members

- **UniqueId** (`Guid`): Auto-generated unique identifier assigned at construction
- **RootElement** (`ElementReference`): Reference to the root HTML element

### Code Examples

**Basic Usage:**
```razor
<BitHeader>
    I'm a Header
</BitHeader>
```

**With Content:**
```razor
<BitHeader>
    My Awesome App
    <ul>
        <li>Settings</li>
        <li>About</li>
        <li>Feedback</li>
    </ul>
</BitHeader>
```

**With Fixed Positioning and Custom Height:**
```razor
<BitHeader Fixed="true" Height="60">
    My Application Header
</BitHeader>
```

### Notes

- The Header uses the current primary background color for its styling
- The `Fixed` parameter is useful for navigation headers that should remain visible while scrolling
- As a `BitComponentBase` descendant, BitHeader supports full accessibility features and HTML attribute customization
- The component is flexible in terms of content and can contain any child Razor markup (text, lists, navigation elements, etc.)

---

## Layout

The **BitLayout** component can be used to create a base UI structure for an application. It provides a structured container with support for header, navigation panel, main content, and footer sections, offering flexible positioning and visibility control.

### Component Description

BitLayout creates a semantic application layout with customizable sections for header, navigation, main content, and footer. It supports sticky positioning for headers/footers, collapsible navigation panels, and full CSS customization through class styles.

### Parameters and Properties

#### Layout-Specific Parameters

| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| `Header` | `RenderFragment?` | Header section content (typically top navigation bar) | null |
| `NavPanel` | `RenderFragment?` | Navigation panel content (sidebar/navigation menu) | null |
| `Main` | `RenderFragment?` | Main content area of the application | null |
| `Footer` | `RenderFragment?` | Footer section content (copyright, links, etc.) | null |
| `NavPanelWidth` | `int` | Width of the navigation panel in pixels | - |
| `HideNavPanel` | `bool` | Controls visibility of the navigation panel | false |
| `ReverseNavPanel` | `bool` | Positions nav panel on the right side instead of left | false |
| `StickyHeader` | `bool` | Enables fixed/sticky positioning for the header | false |
| `StickyFooter` | `bool` | Enables fixed/sticky positioning for the footer | false |
| `Classes` | `BitLayoutClassStyles?` | Custom CSS classes for layout sections | null |

#### Inherited from BitComponentBase

- `Class` (string?): Custom CSS class for the root element
- `Style` (string?): Inline CSS styles
- `Id` (string?): HTML element ID
- `AriaLabel` (string?): Accessibility label
- `Dir` (BitDir): Text direction (Ltr/Rtl)
- `TabIndex` (int?): Tab order index
- `IsEnabled` (bool): Component enabled state
- `HtmlAttributes` (Dictionary<string, object>?): Additional HTML attributes
- `Visibility` (BitVisibility): Component visibility state

### Layout Sections

The Layout component consists of the following customizable areas:

- **Root**: Main container wrapper
- **Header**: Top section (typically fixed/sticky)
- **NavPanel**: Side navigation panel (left or right, collapsible)
- **Main**: Primary content area (scrollable)
- **Footer**: Bottom section (optionally sticky)

### BitLayoutClassStyles

Custom CSS classes can be applied to each section:

```csharp
Classes = new BitLayoutClassStyles 
{ 
    Root = "custom-root",
    Header = "custom-header",
    NavPanel = "custom-nav",
    Main = "custom-main",
    Footer = "custom-footer"
}
```

### Code Examples

#### Basic Layout Structure

```csharp
<BitLayout>
    <Header>Header Content</Header>
    <Main>Main Content</Main>
    <Footer>Footer Content</Footer>
</BitLayout>
```

#### Navigation Panel Configuration

```csharp
<BitLayout NavPanelWidth="250">
    <Header>Header</Header>
    <NavPanel>Navigation Menu</NavPanel>
    <Main>Main Content</Main>
    <Footer>Footer</Footer>
</BitLayout>
```

#### Sticky Header and Footer

```csharp
<BitLayout StickyHeader="true" StickyFooter="true">
    <Header>Fixed Header</Header>
    <Main>Scrollable Content</Main>
    <Footer>Fixed Footer</Footer>
</BitLayout>
```

#### Reversed Navigation Panel (Right Side)

```csharp
<BitLayout NavPanelWidth="250" ReverseNavPanel="true">
    <Header>Header</Header>
    <NavPanel>Navigation (Right Side)</NavPanel>
    <Main>Main Content</Main>
    <Footer>Footer</Footer>
</BitLayout>
```

#### Toggle Navigation Panel Visibility

```csharp
<BitLayout NavPanelWidth="250" HideNavPanel="@isNavPanelHidden">
    <Header>Header</Header>
    <NavPanel>Navigation</NavPanel>
    <Main>Main Content</Main>
    <Footer>Footer</Footer>
</BitLayout>

@code {
    private bool isNavPanelHidden = false;
}
```

#### Custom Styling

```csharp
<BitLayout Classes="new BitLayoutClassStyles 
{ 
    Root = "custom-root",
    Header = "custom-header",
    NavPanel = "custom-nav",
    Main = "custom-main",
    Footer = "custom-footer"
}">
    <Header>Header</Header>
    <NavPanel>Navigation</NavPanel>
    <Main>Main Content</Main>
    <Footer>Footer</Footer>
</BitLayout>
```

#### Complete Feature Example

```csharp
<BitLayout 
    StickyHeader="true" 
    StickyFooter="true"
    NavPanelWidth="300"
    ReverseNavPanel="false"
    HideNavPanel="false"
    Class="app-layout"
    Dir="BitDir.Ltr"
    AriaLabel="Application Layout">
    
    <Header>
        <h1>My Application</h1>
        <nav>Top Navigation</nav>
    </Header>
    
    <NavPanel>
        <nav>
            <ul>
                <li><a href="/">Home</a></li>
                <li><a href="/about">About</a></li>
                <li><a href="/contact">Contact</a></li>
            </ul>
        </nav>
    </NavPanel>
    
    <Main>
        <p>Main page content goes here</p>
    </Main>
    
    <Footer>
        <p>© 2025 Company Name. All rights reserved.</p>
    </Footer>
</BitLayout>
```

---

## Summary

The BitLayout component provides a flexible, semantic layout structure for Blazor applications with support for:
- Multi-section layouts (header, sidebar, main, footer)
- Sticky positioning for headers and footers
- Collapsible/hideable navigation panels
- Customizable panel positioning (left/right)
- Full CSS customization through class styles
- Accessibility features (AriaLabel, Dir support)

This component is essential for creating consistent application UI structures in Bit.BlazorUI-based projects.

---

## Spacer

The **BitSpacer** component is used to generate space between other components. It provides a simple way to create either fixed-width spacing (in pixels) or flexible spacing in your layout.

### Component Description

The BitSpacer is a layout utility component designed to create consistent spacing between UI elements. You can configure it to use a set width (specified in pixels) or create a space with flexible width that adapts to available space.

### Parameters

#### BitSpacer Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Width` | `int?` | `null` | Gets or sets the width of the spacer in pixels. When null, the spacer uses flexible width. |

#### Inherited from BitComponentBase

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaLabel` | `string?` | `null` | Accessible label for assistive technologies |
| `Class` | `string?` | `null` | CSS class name(s) to apply to the component |
| `Dir` | `BitDir?` | `null` | Text directionality: `Ltr` (left-to-right), `Rtl` (right-to-left), or `Auto` |
| `HtmlAttributes` | `Dictionary<string, object>` | `new Dictionary<string, object>()` | Additional HTML attributes to apply to the root element |
| `Id` | `string?` | `null` | Unique identifier for the root HTML element |
| `IsEnabled` | `bool` | `true` | Indicates whether the component responds to user interaction |
| `Style` | `string?` | `null` | CSS style string to apply to the component |
| `TabIndex` | `string?` | `null` | Tab order index for keyboard navigation |
| `Visibility` | `BitVisibility` | `BitVisibility.Visible` | Visibility state: `Visible`, `Hidden`, or `Collapsed` |

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| `UniqueId` | `Guid` | Read-only unique identifier assigned at component construction |
| `RootElement` | `ElementReference` | Reference to the root HTML element for DOM access |

### Enumerations

**BitVisibility:**
- `Visible` (0): Content is rendered and visible
- `Hidden` (1): Content hidden but space remains reserved (CSS: `visibility:hidden`)
- `Collapsed` (2): Component completely hidden and takes no space (CSS: `display:none`)

**BitDir:**
- `Ltr` (0): Left-to-right text directionality
- `Rtl` (1): Right-to-left text directionality
- `Auto` (2): User agent determines directionality automatically

### Usage Example

```blazor
@* Fixed-width spacer (32 pixels) *@
<BitSpacer Width="32" />

@* Flexible spacer (uses available space) *@
<BitSpacer />

@* Spacer with additional styling *@
<BitSpacer Width="16" Class="my-custom-class" Style="background-color: #f0f0f0;" />

@* Hidden spacer (no space taken) *@
<BitSpacer Width="20" Visibility="BitVisibility.Collapsed" />
```

---

## Stack

The **BitStack** component is a container-type component that abstracts the implementation of flexbox in order to define the layout of its children components. It provides a flexible, responsive way to arrange child elements either vertically or horizontally with fine-grained control over spacing, alignment, and sizing.

### Component Overview

The BitStack component simplifies flexbox layout patterns by providing intuitive properties for common layout scenarios without requiring direct CSS manipulation.

### Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Horizontal` | `bool` | `false` | Renders children in horizontal direction (side-by-side) instead of vertically |
| `Vertical` | `bool` | `true` | Renders children vertically (stacked top-to-bottom) - default behavior |
| `Reversed` | `bool` | `false` | Reverses the direction of child rendering (bottom-to-top for vertical, right-to-left for horizontal) |
| `Gap` | `string?` | `null` | Controls spacing between children. Can specify row gap, optionally column gap (e.g., `"1rem"` or `"1rem 2rem"`) |
| `Wrap` | `bool` | `false` | Allows children to wrap across multiple rows or columns when space is constrained |
| `Grow` | `string?` | `null` | Customizable flex-grow proportion for proportional sizing of children |
| `Grows` | `bool` | `false` | Enables auto flex-grow to fill available space proportionally across siblings |
| `FillContent` | `bool` | `false` | Expands children to fill parent dimensions |
| `Alignment` | `BitAlignment?` | `null` | Controls both horizontal and vertical alignment of children simultaneously |
| `HorizontalAlign` | `BitAlignment?` | `null` | Controls horizontal alignment of children |
| `VerticalAlign` | `BitAlignment?` | `null` | Controls vertical alignment of children |
| `AutoSize` | `bool` | `false` | Auto-sizes the component to fit content |
| `AutoWidth` | `bool` | `false` | Auto-sizes the width to fit content |
| `AutoHeight` | `bool` | `false` | Auto-sizes the height to fit content |

### Alignment Options

The `BitAlignment` enum provides the following values for alignment control:

- `Start` - Align items to the start of the container
- `End` - Align items to the end of the container
- `Center` - Center items within the container
- `SpaceBetween` - Distribute items with space between them
- `SpaceAround` - Distribute items with equal space around them
- `SpaceEvenly` - Distribute items with equal space between and around them
- `Baseline` - Align items along their baseline
- `Stretch` - Stretch items to fill available space

### Stacking Modes

#### Vertical Stacking (Default)
Children stack from top to bottom. This is the default behavior when `Horizontal` is not specified.

```csharp
<BitStack>
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</BitStack>
```

#### Horizontal Stacking
Children arrange side-by-side from left to right.

```csharp
<BitStack Horizontal="true">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</BitStack>
```

#### Reversed Direction
Flips the direction of child rendering.

```csharp
<BitStack Horizontal="true" Reversed="true">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</BitStack>
```

### Gap and Spacing

Control spacing between children using the `Gap` property:

```csharp
<BitStack Gap="1rem">
    <div>Item 1</div>
    <div>Item 2</div>
</BitStack>
```

Specify row and column gaps separately:

```csharp
<BitStack Gap="1rem 2rem">
    <div>Item 1</div>
    <div>Item 2</div>
</BitStack>
```

### Alignment Examples

**Center Alignment:**
```csharp
<BitStack HorizontalAlign="BitAlignment.Center" VerticalAlign="BitAlignment.Center">
    <div>Centered Content</div>
</BitStack>
```

**Space-Between Distribution:**
```csharp
<BitStack Horizontal="true" HorizontalAlign="BitAlignment.SpaceBetween">
    <div>Left Item</div>
    <div>Right Item</div>
</BitStack>
```

**Combined Alignment:**
```csharp
<BitStack HorizontalAlign="BitAlignment.Center" VerticalAlign="BitAlignment.SpaceBetween">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</BitStack>
```

### Flexible Growth

Enable proportional growth to fill available space:

```csharp
<BitStack Grows="true">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</BitStack>
```

Custom flex-grow proportions:

```csharp
<BitStack Grow="1 2 1">
    <div>Item 1 (1x)</div>
    <div>Item 2 (2x)</div>
    <div>Item 3 (1x)</div>
</BitStack>
```

### Wrapping and Responsive Layout

Allow children to wrap when space is constrained:

```csharp
<BitStack Horizontal="true" Wrap="true" Gap="1rem">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
    <div>Item 4</div>
</BitStack>
```

### Base Component Properties

BitStack inherits from `BitComponentBase` and supports the following additional properties:

- `Class` - Custom CSS class names
- `Style` - Inline CSS styles
- `Id` - HTML element ID
- `AriaLabel` - Accessibility label
- `Dir` - Text direction (`BitDir.Ltr`, `BitDir.Rtl`, `BitDir.Auto`)
- `TabIndex` - Tab order
- `IsEnabled` - Enable/disable state
- `Visibility` - Component visibility (`Visible`, `Hidden`, `Collapsed`)
- `HtmlAttributes` - Additional HTML attributes

### Summary

The BitStack component is a powerful, flexible layout primitive that significantly simplifies flexbox-based layouts in Blazor applications. It provides semantic properties for common layout patterns while remaining flexible enough for advanced use cases. By combining properties like `Horizontal`, `Gap`, `Alignment`, and `Wrap`, developers can create responsive, well-structured layouts without writing custom CSS or flexbox rules.

---

# Lists

## BasicList

### Component Description
BitBasicList is a base component designed for rendering large sets of items efficiently. It is agnostic of layout, tile component type, and selection management, making it highly flexible for various data presentation scenarios.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `ICollection<TItem>` | Empty array | Collection of items to display in the list |
| `ItemSize` | `float` | 50 | Height of each item in pixels |
| `RowTemplate` | `RenderFragment<TItem>` | null | Template fragment that defines how each item is rendered |
| `EmptyContent` | `RenderFragment` | null | Content to display when the items collection is empty |
| `Virtualize` | `bool` | false | Enables virtualization for efficient rendering of large lists |
| `OverscanCount` | `int` | 3 | Number of additional items to render outside the visible region for smooth scrolling |
| `LoadMore` | `bool` | false | Activates pagination/load-more functionality |
| `LoadMoreSize` | `int` | 20 | Number of items to load per pagination request |
| `LoadMoreText` | `string` | null | Custom text label for the load-more button |
| `LoadMoreTemplate` | `RenderFragment<bool>` | null | Custom template for the load-more button layout |
| `ItemsProvider` | `BitBasicListItemsProvider<TItem>` | null | Async data loading function for dynamic data binding |

### Key Features

#### Virtualization
- **Basic Virtualization**: Efficiently renders only visible items in the viewport to optimize performance with large datasets
- **Overscan Configuration**: Configurable overscan count (default 3) to render additional items outside the visible region, ensuring smooth scrolling experience
- **Placeholder Templates**: Supports placeholder rendering during data loading

#### Load More Patterns
- **Button-Triggered Loading**: Simple load-more button functionality
- **Custom Variants**: Supports custom text labels and template layouts via `LoadMoreText` and `LoadMoreTemplate`
- **Provider Integration**: Works seamlessly with `ItemsProvider` for async data loading
- **Virtualization Compatible**: Load-more functionality integrates with virtualization for seamless large dataset handling

#### Additional Capabilities
- **RTL Support**: Full right-to-left language compatibility for international applications
- **Customization**: Supports CSS classes and styles for:
  - Root element styling
  - Load button styling
  - Text styling

### Inherited Properties
Inherits from `BitComponentBase`, providing additional properties including:
- `Class` - CSS class names
- `Style` - Inline styles
- `Id` - HTML element ID
- `AriaLabel` - Accessibility label
- `Dir` - Text direction (ltr/rtl)
- `IsEnabled` - Enable/disable state
- `TabIndex` - Tab order
- `Visibility` - Visibility control
- `HtmlAttributes` - Additional HTML attributes

### Usage Example

```csharp
<BitBasicList TItem="Item" 
              Items="Items" 
              ItemSize="50"
              Virtualize="true"
              OverscanCount="5"
              LoadMore="true"
              LoadMoreSize="20">
    <RowTemplate>
        <div class="item">
            @context.Name - @context.Description
        </div>
    </RowTemplate>
    <EmptyContent>
        <p>No items found</p>
    </EmptyContent>
    <LoadMoreTemplate>
        <button>Load @context more items...</button>
    </LoadMoreTemplate>
</BitBasicList>
```

This documentation provides comprehensive coverage of the BitBasicList component's capabilities, parameters, and usage patterns for implementing efficient list rendering in Blazor applications.

---

## Carousel

### Component Overview

BitCarousel is a slideshow component that enables users to navigate through multiple content items with customizable animations and display options. It supports automatic transitions, manual navigation, infinite scrolling, and comprehensive keyboard/accessibility support.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AnimationDuration` | `double` | `0.5s` | Controls the scrolling animation speed in seconds |
| `AutoPlay` | `bool` | `false` | Enables automatic slide transitions |
| `AutoPlayInterval` | `double` | `2000ms` | Sets the time interval (in milliseconds) between automatic transitions |
| `ChildContent` | `RenderFragment?` | `null` | Contains the carousel items to be displayed |
| `HideDots` | `bool` | `false` | Toggles the visibility of navigation dot indicators |
| `HideNextPrev` | `bool` | `false` | Hides the next/previous navigation buttons |
| `InfiniteScrolling` | `bool` | `false` | Enables looping carousel behavior - when reaching the end, loops back to the beginning |
| `ScrollItemsCount` | `int` | `1` | Number of items scrolled per navigation action |
| `VisibleItemsCount` | `int` | `1` | Number of items displayed simultaneously in the carousel |
| `Dir` | `string` | - | Supports right-to-left (RTL) layout configuration |
| `Class` | `string?` | `null` | Custom CSS classes (inherited from BitComponentBase) |
| `Style` | `string?` | `null` | Inline styles (inherited from BitComponentBase) |
| `Id` | `string?` | `null` | HTML element ID (inherited from BitComponentBase) |
| `AriaLabel` | `string?` | `null` | Accessibility label |
| `TabIndex` | `int?` | `null` | Tab order in document flow |

### Public Methods/API

The component exposes the following public methods for programmatic control:

- **`GoNext()`** - Advance carousel to the next slide
- **`GoPrev()`** - Return carousel to the previous slide
- **`GoTo(int slideNumber)`** - Navigate directly to a specific slide by number
- **`Pause()`** - Stop the AutoPlay functionality
- **`Resume()`** - Restart the AutoPlay after pause

### Events

- **`OnChange`** - Event callback triggered whenever carousel navigation occurs

### Autoplay Configuration

When `AutoPlay` is set to `true`:
- The carousel automatically transitions between slides
- Transition interval is controlled by `AutoPlayInterval` parameter (default: 2000ms)
- Animation duration is determined by `AnimationDuration` parameter (default: 0.5s)
- Can be paused/resumed using `Pause()` and `Resume()` methods

### Navigation Options

1. **Automatic Navigation** - When `AutoPlay = true` with configurable interval
2. **Manual Navigation** - Using programmatic methods (`GoNext()`, `GoPrev()`, `GoTo()`)
3. **UI Navigation** - Visible navigation buttons (unless `HideNextPrev = true`) and dot indicators (unless `HideDots = true`)
4. **Keyboard Navigation** - Full keyboard support included
5. **Infinite Scrolling** - Enabled via `InfiniteScrolling = true` for continuous looping

### Styling & Customization

The component supports CSS customization for:
- Root container and main wrapper
- Navigation buttons (left/right arrows)
- Dot indicator elements
- Current/active dot highlighting
- Custom classes and inline styles

### Accessibility Features

- Full keyboard and assistive technology support
- ARIA labels and semantic HTML
- TabIndex support for navigation flow
- AriaLabel parameter for screen readers

### Responsive Display

- **`VisibleItemsCount`** - Control how many items display at once
- **`ScrollItemsCount`** - Control how many items advance per navigation action
- Supports RTL layouts via `Dir` parameter

---

## Swiper

### Component Description

**BitSwiper** is a touch-enabled slider component that lets people show their slides in a swiping row. It's part of the Bit BlazorUI library's Lists category and provides native swipe gesture support for mobile-optimized interfaces.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AnimationDuration` | `double` | `0.5` | Sets the duration of the scrolling animation in seconds |
| `ChildContent` | `RenderFragment?` | `null` | Items displayed in the swiper |
| `HideNextPrev` | `bool` | `false` | Removes/hides navigation buttons |
| `ScrollItemsCount` | `int` | `1` | Number of items that will be changed on navigation |

### Inherited Base Parameters

| Parameter | Type | Purpose |
|-----------|------|---------|
| `AriaLabel` | `string` | Accessibility label for screen readers |
| `Class` | `string` | CSS class names for styling |
| `Dir` | `BitDir` | Text directionality (`Ltr`, `Rtl`, `Auto`) |
| `Style` | `string` | Inline CSS styles |
| `IsEnabled` | `bool` | Control component responsiveness and interaction |
| `Id` | `string` | Unique element identifier |
| `TabIndex` | `string` | Keyboard navigation order |
| `Visibility` | `BitVisibility` | Display state control |

### Touch/Swipe Options

- **Native Swipe Gesture Support**: Full touch gesture recognition for mobile devices
- **RTL (Right-to-Left) Language Support**: Demonstrated with complete RTL layout implementation
- **Mobile Optimization**: Includes iOS Safari auto-zoom prevention for better mobile UX

### Public Members

| Member | Type | Purpose |
|--------|------|---------|
| `UniqueId` | `Guid` | Auto-generated unique component identifier |
| `RootElement` | `ElementReference` | DOM element reference for direct manipulation |

### Code Examples

**1. Basic Horizontal Scrolling**
```csharp
<BitSwiper>
    <!-- Swiper items here -->
</BitSwiper>
```

**2. Multi-Item Scroll Configuration**
```csharp
<BitSwiper ScrollItemsCount="3" AnimationDuration="0.8">
    <!-- Items scroll 3 at a time -->
</BitSwiper>
```

**3. Hidden Navigation Variant**
```csharp
<BitSwiper HideNextPrev="true">
    <!-- Navigation buttons hidden, swipe only -->
</BitSwiper>
```

**4. RTL Layout Implementation**
```csharp
<BitSwiper Dir="BitDir.Rtl">
    <!-- Right-to-left swiper layout -->
</BitSwiper>
```

### Accessibility & Framework Integration

- Built with **Blazor** component architecture
- Integrates with Bit platform's component ecosystem
- Supports accessibility standards via `AriaLabel` parameter
- Full keyboard navigation support via `TabIndex`
```

This complete documentation covers all component parameters, touch/swipe capabilities, properties with types, and practical code examples for implementing BitSwiper in your Blazor applications.

---

## Timeline

### Component Description

The **BitTimeline** component organizes and displays events or data chronologically in a linear fashion. It features points or segments representing individual items with associated details or actions. The component is highly flexible and supports multiple configuration approaches, visual styles, and customization options.

### Key Features

- **Multi-API Support**: Accepts items via BitTimelineItem class, custom generic classes, or BitTimelineOption components
- **Orientation Options**: Vertical (default) or horizontal layouts
- **Visual Variants**: Fill, Outline, and Text styling
- **Customization**: Icons, reversed direction, custom templates, and color options
- **Accessibility**: RTL support and ARIA label capabilities
- **Item States**: Individual items can be enabled/disabled and customized independently

### Core Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IEnumerable<TItem>` | `new List<TItem>()` | List of timeline items to display |
| `Horizontal` | `bool` | `false` | Enables horizontal layout (default is vertical) |
| `Reversed` | `bool` | `false` | Reverses the direction of timeline items |
| `Variant` | `BitVariant` | `null` | Visual style variant (Fill, Outline, or Text) |
| `Color` | `BitColor` | `null` | Color theme for the timeline |
| `Size` | `BitSize` | `null` | Size variant (Small, Medium, Large) |
| `OnItemClick` | `EventCallback<TItem>` | — | Event handler triggered when an item is clicked |

### Timeline Item Configuration (BitTimelineItem)

Individual timeline items can be configured with the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `PrimaryText` | `string` | Main label or title for the timeline item |
| `SecondaryText` | `string` | Supporting text or subtitle for the item |
| `IconName` | `string` | Icon to display at the timeline point |
| `DotTemplate` | `RenderFragment` | Custom template for the timeline dot appearance |
| `PrimaryContent` | `RenderFragment` | Custom HTML template for primary content area |
| `SecondaryContent` | `RenderFragment` | Custom HTML template for secondary content area |
| `IsEnabled` | `bool` | Controls whether the item is enabled or disabled |
| `Class` | `string` | CSS class for per-item styling |
| `Style` | `string` | Inline CSS styles for per-item customization |

### Color Options

The Timeline component supports the following color states:
- Primary
- Secondary
- Tertiary
- Info
- Success
- Warning
- SevereWarning
- Error

### Implementation Approaches

The Timeline component can be implemented in three different ways:

1. **Direct BitTimelineItem Objects**: Create and bind a collection of BitTimelineItem instances directly to the Items parameter
2. **Custom Classes with NameSelectors**: Map properties from custom domain objects to timeline item properties using selector functions
3. **BitTimelineOption Child Components**: Use nested BitTimelineOption components as a declarative approach to building timeline items

### Usage Notes

- **Default Orientation**: Vertical layout is the default; set `Horizontal="true"` for horizontal display
- **Item Interaction**: Use the `OnItemClick` event callback to handle clicks on individual timeline items
- **Custom Rendering**: Leverage `DotTemplate`, `PrimaryContent`, and `SecondaryContent` render fragments for advanced customization
- **Per-Item Control**: Individual items can be disabled via the `IsEnabled` property or styled with custom `Class` and `Style` properties
- **Size Variants**: Available sizes are Small, Medium, and Large for responsive designs

---

**Source**: [Bit.BlazorUI Timeline Component Documentation](https://blazorui.bitplatform.dev/components/timeline)

---

# Navs

## Breadcrumb

### Description
The Breadcrumb component provides navigational aid in your app or site. Breadcrumbs indicate the current page's location within a hierarchy and enable quick access to higher levels.

### Component Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IList<TItem>` | `[]` | Collection of breadcrumb items to display |
| `MaxDisplayedItems` | `uint` | `0` | Maximum number of breadcrumbs to display before collapsing |
| `OverflowIndex` | `uint` | `0` | Index position where the overflow item (collapsed items) should be placed |
| `OverflowIconName` | `string` | `More` | Icon name to display for collapsed items button |
| `DividerIconName` | `string?` | `null` | Icon name to use as separator between breadcrumb items |
| `IsEnabled` | `bool` | `true` | Controls whether the component is interactive |

### Item Configuration (BitBreadcrumbItem)

Each breadcrumb item can be configured with the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string?` | Unique identifier for the item |
| `Text` | `string?` | Display text for the breadcrumb item |
| `Href` | `string?` | Navigation URL the item links to |
| `IconName` | `string?` | Icon to display alongside the item text |
| `IsSelected` | `bool` | Indicates whether the item is currently selected |
| `IsEnabled` | `bool` | Controls whether the individual item is interactive |
| `OnClick` | `Action` | Callback handler when the item is clicked |
| `Class` | `string?` | Custom CSS class for styling |
| `Style` | `string?` | Custom inline CSS styles |

### Multi-API Support

BitBreadcrumb accepts items through three approaches:
1. **BitBreadcrumbItem class** - Standard item objects
2. **Custom Generic class** - Define your own item type
3. **BitBreadcrumbOption component** - Declarative component-based configuration

### Templates

- **ItemTemplate** - Customize rendering of individual breadcrumb items
- **OverflowTemplate** - Customize the display of collapsed items
- **DividerIconTemplate** - Customize the separator between items

### Events

- **OnItemClick** - Callback fires when users select breadcrumb items, allowing you to update the `SelectedItemStyle` accordingly

---

## DropMenu

The **BitDropMenu** component is a versatile dropdown menu component for Blazor applications that enables creation of a button opening a callout or dropdown menu. It's ideal for navigation menus, action lists, and interactive elements requiring dropdown functionality.

### Component Description

BitDropMenu provides a flexible dropdown/menu interface that combines a trigger button with a dismissible callout containing menu items or custom content. It supports various styling options, responsive layouts, icon customization, and event handling.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string?` | `null` | Display text shown in the dropdown header/button |
| `IconName` | `string?` | `null` | Icon displayed inside the header before the text |
| `ChevronDownIcon` | `string?` | `null` | Icon shown for the chevron-down section (typically dropdown indicator) |
| `IsOpen` | `bool` | `false` | Controls whether the callout/dropdown menu is currently open or closed |
| `Transparent` | `bool` | `false` | When `true`, makes the header background transparent |
| `Responsive` | `bool` | `false` | Enables responsive rendering behavior on small screens |
| `Template` | `RenderFragment?` | `null` | Custom render fragment for header content, overrides default header layout |
| `ChildContent` / `Body` | `RenderFragment?` | `null` | Content rendered inside the callout/dropdown menu |
| `ScrollContainerId` | `string?` | `null` | Element ID of a scrollable container (for positioning calculations) |

### Events

- **`OnClick`** - Triggered when the dropdown menu is activated/clicked
- **`OnDismiss`** - Triggered when the dropdown menu closes or is dismissed

### Styling & Customization

The **`BitDropMenuClassStyles`** property allows customization of the following CSS classes:

- `Root` - Root container element
- `Opened` - Applied when menu is in open state
- `Button` - The trigger/header button element
- `Icon` - Icon styling
- `Text` - Text content styling
- `ChevronDown` - Chevron-down indicator styling
- `Overlay` - Backdrop/overlay styling
- `Callout` - Dropdown menu container styling

### Features

- **Responsive Layout Support** - Automatically adjusts layout on smaller screens when `Responsive="true"`
- **Icon Support** - Customizable icons for both the header and chevron indicator
- **Transparent Styling** - Optional transparent header background
- **RTL Support** - Right-to-left language support included
- **Custom Templates** - Full header customization via `Template` parameter
- **Event Handling** - Click and dismiss event callbacks for interaction control
- **Disabled States** - Support for disabled state rendering

### Common Use Cases

1. **Navigation Menus** - Create primary or secondary navigation dropdowns
2. **Action Lists** - Present multiple action options when space is limited
3. **Filter Controls** - Dropdown menus for filtering and sorting options
4. **Context Menus** - Interactive menu options triggered by button clicks
5. **Option Selection** - Multi-option selection interfaces

### Code Example Structure

The component supports implementations such as:

- Basic dropdown with text and items
- Icon-based dropdowns
- Disabled state dropdowns
- Transparent header styling
- Custom templated headers
- Responsive mobile-friendly layouts
- Event-driven interactions
- RTL/localized menu support

### Integration Notes

When using BitDropMenu in your Blazor Server or Blazor WebAssembly application:
- Set `IsOpen` to control menu state programmatically
- Use `OnDismiss` callback to reset state when menu closes
- Leverage `Template` for complex custom headers
- Apply `BitDropMenuClassStyles` for consistent theming with your application design system
- Use `ScrollContainerId` when dropdown appears near scrollable content

---

## Nav

### Component Description

**BitNav** is a navigation pane component that provides links to the main areas of an app or site. It can also be used as a TreeView to display parent-child data hierarchically. The component supports flexible item binding through multiple APIs and offers extensive customization for navigation behaviors and visual presentation.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **Items** | `IList<TItem>` | `new List<TItem>()` | Collection of items to display in the navigation pane |
| **SelectedItem** | `TItem?` | `null` | Currently selected item |
| **Mode** | `BitNavMode` | `Automatic` | Navigation handling approach (Automatic or Manual) |
| **AllExpanded** | `bool` | `false` | Expands all items on initial render |
| **SingleExpand** | `bool` | `false` | When enabled, only one branch can be expanded at a time |
| **IconOnly** | `bool` | `false` | Renders only icons without text labels |
| **FitWidth** | `bool` | `false` | Fits width to available content |
| **FullWidth** | `bool` | `false` | Expands to full container width |
| **IndentValue** | `int` | `16` | Indentation padding per depth level in pixels |
| **RenderType** | `BitNavRenderType` | `Normal` | Rendering mode (Normal or Grouped) |

### BitNavItem Structure

The `BitNavItem` class represents individual navigation items with the following properties:

| Property | Type | Purpose |
|----------|------|---------|
| **Text** | `string` | Display text for the navigation item |
| **Description** | `string?` | Additional descriptive text |
| **IconName** | `string?` | Icon identifier/name |
| **Url** | `string?` | Navigation URL |
| **Target** | `string?` | Link target (e.g., "_blank") |
| **ForceAnchor** | `bool` | Forces anchor navigation behavior |
| **ChildItems** | `List<BitNavItem>` | Nested child items for hierarchical structure |
| **IsExpanded** | `bool` | Current expansion state |
| **IsEnabled** | `bool` | Enable/disable the item |
| **AriaCurrent** | `string?` | ARIA attribute for current page |
| **AriaLabel** | `string?` | ARIA label for accessibility |
| **CollapseAriaLabel** | `string?` | ARIA label for collapse action |
| **ExpandAriaLabel** | `string?` | ARIA label for expand action |
| **Match** | `BitNavMatch` | URL matching behavior (Exact, Prefix, Regex, Wildcard) |
| **Class** | `string?` | Custom CSS class |
| **Style** | `string?` | Inline styles |
| **Data** | `IDictionary<string, object>?` | Custom data attributes |

### Multi-API Item Binding

The component supports three methods for providing items:

1. **BitNavItem Class**: Traditional approach using strongly-typed BitNavItem objects
2. **Custom Generic Class**: Bind to custom model classes with generic type parameters
3. **BitNavOption Component**: Markup-based child content approach

### BitNavOption Component

Used for declarative item definition within component markup:

| Property | Type | Purpose |
|----------|------|---------|
| **Text** | `string` | Display text for the option |
| **IconName** | `string?` | Associated icon identifier |
| **Description** | `string?` | Additional descriptive information |
| **ChildContent** | `RenderFragment?` | Nested BitNavOption components |
| **Url** | `string?` | Navigation link URL |

### Enumerations

**BitNavMode**
- `Automatic`: URL-based navigation handling
- `Manual`: Parent-controlled navigation behavior

**BitNavMatch**
- `Exact`: Exact URL match required
- `Prefix`: Prefix matching
- `Regex`: Regular expression matching
- `Wildcard`: Wildcard pattern matching

**BitNavRenderType**
- `Normal`: Standard rendering
- `Grouped`: Root elements rendered as grouped list

**BitVisibility**
- `Visible`: Item is visible
- `Hidden`: Item is hidden but takes space
- `Collapsed`: Item is hidden and takes no space

### Events & Callbacks

| Event | Trigger |
|-------|---------|
| **OnItemClick** | Triggered when an item is clicked |
| **OnSelectItem** | Triggered when an item is selected |
| **OnItemToggle** | Triggered when group headers expand or collapse |

### Public Methods

| Method | Purpose |
|--------|---------|
| **ExpandAll()** | Expands all items and children (not available in SingleExpand mode) |
| **CollapseAll()** | Collapses all items and children |
| **ToggleItem()** | Toggles the expansion state of an item |

### Tree Structure Example

```
Root Level Item 1
├── Child Item 1.1
├── Child Item 1.2
│   ├── Grandchild Item 1.2.1
│   └── Grandchild Item 1.2.2
└── Child Item 1.3

Root Level Item 2
├── Child Item 2.1
└── Child Item 2.2
```

### Configuration Examples

**AllExpanded Mode**: All items and their children are expanded on initial component render

**SingleExpand Mode**: Only one branch can remain expanded at any given time; expanding a new branch collapses the previously expanded one

**IconOnly Mode**: Renders exclusively icon representations without accompanying text labels

**Grouped RenderType**: Root-level elements are rendered in a manner that resembles a grouped list presentation

### Usage Recommendations

- Use `AllExpanded` for small navigation trees with limited depth
- Use `SingleExpand` for large hierarchies to improve UI clarity
- Use `IconOnly` mode when space is limited (sidebar, mobile views)
- Use `Grouped` RenderType for organizing related navigation sections
- Leverage `IndentValue` to adjust visual hierarchy spacing for different layouts

---

## NavBar

### Component Description

The **BitNavBar** is a multi-API navigation component that provides navigation links to the main areas of an app. It functions as a tab panel with support for NavMenu and TabPanel variations, accepting items through multiple approaches for flexible integration into any Blazor application.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Items` | `IList<TItem>` | `new List<TItem>()` | Collection of navigation items to display |
| `SelectedItem` | `TItem?` | `null` | Currently selected/active item |
| `DefaultSelectedItem` | `TItem?` | `null` | Initially selected item (used in manual mode) |
| `Mode` | `BitNavMode` | `Automatic` | Determines selection handling: `Automatic` (URL-based) or `Manual` (parent-controlled) |
| `IconOnly` | `bool` | `false` | Renders only item icons without text labels |
| `FitWidth` | `bool` | `false` | Adjusts component width based on content size |
| `FullWidth` | `bool` | `false` | Expands component to fill container width |
| `Reselectable` | `bool` | `false` | Allows re-triggering select events when clicking already-selected item |
| `Color` | `BitColor?` | `null` | Visual styling color (Primary, Secondary, Success, Error, Warning variants) |

### Navigation Item Properties

Each navigation item supports the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `Text` | `string` | Display label for the navigation item |
| `Url` | `string?` | Navigation link/route URL |
| `IconName` | `string?` | Associated icon name |
| `Key` | `string?` | Unique identifier for the item |
| `IsEnabled` | `bool` | Enable/disable state for the item |
| `Target` | `string?` | Link target behavior (e.g., `_blank`, `_self`) |
| `Title` | `string?` | Tooltip text displayed on hover |
| `Template` | `RenderFragment?` | Custom rendering fragment for item content |
| `AdditionalUrls` | `IEnumerable<string>?` | Alternative matching URLs for selection state |
| `Class` | `string?` | Custom CSS class styling |
| `Style` | `string?` | Custom inline CSS styling |

### Item Input Methods

The component accepts navigation items via three different approaches:

1. **BitNavBarItem class** - Direct item model usage with strongly-typed configuration
2. **Custom Generic classes** - Type-flexible item binding for custom models
3. **BitNavBarOption component** - Child component declaration for declarative syntax

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnItemClick` | `EventCallback<TItem>` | Triggered when an item is clicked |
| `OnSelectItem` | `EventCallback<TItem>` | Triggered when an item is selected |

### Styling & Customization

**Class and Style Properties:**
- `Classes` / `Styles` - Accept `BitNavClassStyles?` for customizing:
  - `Root` - Root container styles
  - `Container` - Items container styles
  - `Item` - Individual item styles
  - `ItemIcon` - Item icon styles
  - `ItemText` - Item text label styles
  - `SelectedItem` - Currently selected item styles

**Display Modes:**
- `IconOnly` - Renders icons exclusively without text labels
- `FitWidth` - Content-based width sizing
- `FullWidth` - Container-spanning layout

**Color Theming:**
The component supports color variants including: Primary, Secondary, Success, Error, Warning, and supports RTL (Right-to-Left) functionality.

### Key Features

- **Multi-selection approach** - Support for BitNavBarItem class, custom generic classes, or BitNavBarOption components
- **Flexible selection modes** - Automatic (URL-based) or Manual (parent-controlled) selection handling
- **Custom item templates** - ItemTemplate property (`RenderFragment<TItem>?`) enables custom rendering of navigation items
- **State management** - Control current selection via `SelectedItem` property with event callbacks
- **Disabled states** - Individual items can be disabled via `IsEnabled` property
- **URL matching** - Primary `Url` plus `AdditionalUrls` support for flexible route matching
- **Styling flexibility** - CSS class and style overrides via BitNavBarClassStyles properties
- **Color theming** - Multiple color variants for visual consistency
- **RTL support** - Right-to-Left language support
- **Tooltip support** - `Title` property for hover tooltips
- **Icon support** - Built-in IconName property for integration with icon libraries

### Tab Panel Integration

The BitNavBar component can function as a tab panel with the following capabilities:

- Works seamlessly with tab-based navigation patterns
- Supports NavMenu variations for hierarchical navigation
- Can integrate with TabPanel components for content-switching scenarios
- Maintains selection state across tab interactions
- Supports reselection of already-selected tabs when `Reselectable` is enabled

---

## Pagination

The Pagination component helps users easily navigate through content, allowing swift browsing across multiple pages or sections. It is commonly used in lists, tables, and content-rich interfaces.

### Component Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Count` | `int` | `1` | Total number of pages |
| `SelectedPage` | `int` | `0` | Current selected page (0-indexed) |
| `DefaultSelectedPage` | `int` | `0` | Initially selected page |
| `BoundaryCount` | `int` | `2` | Number of items displayed at the start and end of pagination |
| `MiddleCount` | `int` | `3` | Number of pages shown in the middle section |
| `ShowFirstButton` | `bool` | `false` | Display the first-page navigation button |
| `ShowLastButton` | `bool` | `false` | Display the last-page navigation button |
| `ShowNextButton` | `bool` | `true` | Display the next-page navigation button |
| `ShowPreviousButton` | `bool` | `true` | Display the previous-page navigation button |
| `FirstIcon` | `string?` | `null` | Icon for the first-page button |
| `LastIcon` | `string?` | `null` | Icon for the last-page button |
| `NextIcon` | `string?` | `null` | Icon for the next-page button |
| `PreviousIcon` | `string?` | `null` | Icon for the previous-page button |
| `Color` | `BitColor?` | `null` | Visual color scheme (Primary, Secondary, Tertiary, Info, Success, Warning, SevereWarning, Error) |
| `Size` | `BitSize?` | `null` | Button sizing option (Small, Medium, Large) |
| `Variant` | `BitVariant?` | `null` | Visual styling variant (Fill, Standard/Outline, Text) |
| `Classes` | `BitPaginationClassStyles?` | `null` | Custom CSS classes for component parts |
| `Styles` | `BitPaginationClassStyles?` | `null` | Custom CSS styles for component parts |

### Events

| Event | Type | Description |
|-------|------|-------------|
| `OnChange` | `EventCallback<int>` | Fires when page selection changes, returning the newly selected page number |

### Customizable Style/Class Elements

The component supports styling customization for the following elements:

- `Root` - Main container
- `Button` - Individual page buttons
- `Ellipsis` - Ellipsis indicator
- `SelectedButton` - Currently selected page button
- `FirstButton` - First-page button
- `FirstButtonIcon` - Icon within first-page button
- `PreviousButton` - Previous-page button
- `PreviousButtonIcon` - Icon within previous-page button
- `NextButton` - Next-page button
- `NextButtonIcon` - Icon within next-page button
- `LastButton` - Last-page button
- `LastButtonIcon` - Icon within last-page button

### Features

- **Multiple Variants**: Fill, Standard (Outline), and Text styling options
- **Configurable Navigation**: Show/hide first, last, next, and previous buttons independently
- **Custom Iconography**: Full support for custom icons on navigation buttons
- **Color Schemes**: Multiple color options for different use cases
- **Size Options**: Small, Medium, and Large button sizing
- **Boundary/Middle Configuration**: Control pagination layout with boundary and middle item counts
- **Data Binding**: One-way and two-way data binding support
- **RTL Support**: Full right-to-left language support
- **Inherited Properties**: Standard BitComponentBase properties including AriaLabel, Class, Dir, HtmlAttributes, Id, IsEnabled, Style, TabIndex, and Visibility

---

## Pivot

The **BitPivot** component enables navigation between multiple content categories using text headers. It's used for navigating frequently accessed, distinct content categories and provides flexible header positioning, styling, and overflow handling.

### Component Description

The Pivot control renders a tabbed interface with customizable headers (`Tab` or `Link` style) that allow users to switch between different content sections. It supports various configurations including header positioning, sizing, alignment, and overflow behaviors for responsive layouts.

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `DefaultSelectedKey` | `string?` | Sets the initial selected tab using its Key identifier |
| `SelectedKey` | `string?` | Programmatically controls which tab is active |
| `HeaderType` | `BitPivotHeaderType` | Rendering style: `Tab` or `Link` |
| `Position` | `BitPivotPosition` | Header placement: `Top`, `Bottom`, `Start` (left), or `End` (right) |
| `Size` | `BitSize?` | Header sizing: `Small`, `Medium`, or `Large` |
| `Alignment` | `BitAlignment?` | Controls header alignment across the container |
| `HeaderOnly` | `bool` | When `true`, skips rendering tab content panels entirely (header-only mode) |
| `MountAll` | `bool` | Renders all tabs upfront, hiding non-selected ones via CSS instead of unmounting |
| `OverflowBehavior` | `BitOverflowBehavior` | Handles overflow scenarios: `None`, `Menu` (dropdown), or `Scroll` |

### Pivot Item Structure

Each `BitPivotItem` within the Pivot component includes:

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string?` | Unique identifier for the pivot item |
| `HeaderText` | `string?` | Display text shown in the header |
| `Header` | `RenderFragment?` | Custom header template for advanced customization |
| `ChildContent` / `Body` | `RenderFragment?` | The content rendered when this tab is active |
| `IconName` | `string?` | Optional icon to display in the header |
| `ItemCount` | `int` | Optional badge number displayed in parentheses next to the header |

### Event Callbacks

- **`OnItemClick`**: Fires when the user clicks any pivot item
- **`OnChange`**: Fires when the selected pivot item changes

### Navigation Patterns

- **Header Positioning**: Place headers at `Top` (default), `Bottom`, `Start` (sidebar), or `End` for flexible layouts
- **HeaderOnly Mode**: Use `HeaderOnly="true"` to render only the navigation headers without content panels
- **Overflow Handling**:
  - `None`: No special handling
  - `Menu`: Overflow items appear in a dropdown menu
  - `Scroll`: Horizontal/vertical scrolling for excess items
- **Programmatic Selection**: Use `SelectedKey` binding to control active tab from code
- **Icon Integration**: Add icons via `IconName` property for visual context

### Styling

Customize individual Pivot elements through the `BitPivotClassStyles` object:

- `Root`: Main Pivot container
- `Header`: Header section container
- `Body`: Content area container
- `HeaderItem`: Individual pivot item header
- `SelectedItem`: Currently active pivot header
- `HeaderItemContent`: Header content wrapper
- `HeaderIcon`: Icon element styling
- `HeaderText`: Header text styling
- `HeaderItemCount`: Badge/count display styling

### Basic Usage Example

```html
<BitPivot DefaultSelectedKey="tab1" HeaderType="BitPivotHeaderType.Tab" Position="BitPivotPosition.Top">
    <BitPivotItem Key="tab1" HeaderText="Home">
        <p>Home content goes here</p>
    </BitPivotItem>
    <BitPivotItem Key="tab2" HeaderText="Profile" IconName="Contact">
        <p>Profile content goes here</p>
    </BitPivotItem>
    <BitPivotItem Key="tab3" HeaderText="Settings" ItemCount="5">
        <p>Settings content goes here</p>
    </BitPivotItem>
</BitPivot>
```

### Advanced Usage with Custom Headers

```html
<BitPivot @bind-SelectedKey="activeTab" OverflowBehavior="BitOverflowBehavior.Menu">
    <BitPivotItem Key="notifications" ItemCount="3">
        <Header>
            <span class="custom-header">Notifications</span>
        </Header>
        <ChildContent>
            <!-- Tab content -->
        </ChildContent>
    </BitPivotItem>
    <BitPivotItem Key="messages" HeaderText="Messages">
        <!-- Tab content -->
    </BitPivotItem>
</BitPivot>
```

### HeaderOnly Mode

When `HeaderOnly="true"`, the Pivot component renders only the navigation headers without any content panels. This is useful for creating navigation-only interfaces:

```html
<BitPivot HeaderOnly="true" Position="BitPivotPosition.Start">
    <BitPivotItem Key="section1" HeaderText="Section 1" />
    <BitPivotItem Key="section2" HeaderText="Section 2" />
    <BitPivotItem Key="section3" HeaderText="Section 3" />
</BitPivot>
```

---

# Notifications

## Badge

The Badge component is a small visual element used to highlight or indicate specific information within a user interface. It can display text, numbers, or icons and can be positioned relative to other content with customizable styling.

### Component Description

The Badge component provides a flexible way to display notifications, counts, or status indicators. It supports multiple colors, sizes, and styles (fill, outline, text), and can be configured to overlap with adjacent content or display as a simple dot indicator.

### Parameters & Properties

| Parameter | Type | Description |
|-----------|------|-------------|
| `ChildContent` | `RenderFragment?` | Content the badge applies to |
| `Content` | `object?` | Badge interior content (supports string or integer types) |
| `Dot` | `bool` | Reduces size and hides content to display as a simple dot indicator |
| `Hidden` | `bool` | Controls visibility of the badge |
| `IconName` | `string?` | Icon name to display within the badge |
| `Max` | `int?` | Maximum value for integer content display (truncates numbers exceeding this value) |
| `OnClick` | `EventCallback` | Click event handler for interactive badges |
| `Overlap` | `bool` | Determines whether badge overlaps on child content |
| `Position` | `BitPosition?` | Badge placement location relative to child content |
| `Size` | `BitSize?` | Badge size: Small, Medium, or Large |
| `Variant` | `BitVariant?` | Visual style variant: Fill (default), Outline, or Text |
| `Color` | `BitColor?` | Visual color variant for the badge |
| `Class` | `string?` | Custom CSS class names |
| `Id` | `string?` | HTML element ID |
| `Style` | `string?` | Inline CSS styles |
| `IsEnabled` | `bool` | Enables or disables the component |
| `Dir` | `string?` | Text direction (LTR/RTL) |
| `TabIndex` | `int?` | Tab order for keyboard navigation |
| `Visibility` | `BitVisibility?` | Visibility state of the component |
| `AriaLabel` | `string?` | Accessibility label |
| `HtmlAttributes` | `Dictionary<string, object>?` | Additional HTML attributes |

### Available Colors

- Primary
- Secondary
- Tertiary
- Info
- Success
- Warning
- SevereWarning
- Error

### Size Options

- Small
- Medium
- Large

### Variant Types

- **Fill** (default) - Solid background with text
- **Outline** - Border style with transparent background
- **Text** - Text-only style without background

### Position Options

The badge can be positioned at 15 different locations relative to child content:

- `TopLeft`, `TopCenter`, `TopRight`
- `TopStart`, `TopEnd`
- `CenterLeft`, `Center`, `CenterRight`
- `CenterStart`, `CenterEnd`
- `BottomLeft`, `BottomCenter`, `BottomRight`
- `BottomStart`, `BottomEnd`

### Key Features

- **Customizable Content**: Display text, numbers, or icons
- **Multiple Color Variants**: 8 predefined color options for different semantic meanings
- **Flexible Sizing**: Three size options (Small, Medium, Large)
- **Flexible Positioning**: 15 positioning options around child content
- **Max Value Capping**: Truncate numeric content to a maximum value
- **Dot Mode**: Display as a simple indicator dot without content
- **Icon Support**: Include icons within the badge
- **Overlap Control**: Choose whether badge overlaps with adjacent content
- **Click Events**: Handle user interactions with `OnClick` callback
- **Show/Hide Control**: Toggle visibility with the `Hidden` property
- **Accessibility**: Full support for ARIA labels and keyboard navigation

---

## Message

A Message component displays errors, warnings, or important information to users. For example, if a file failed to upload, an error message should appear.

### Component Description

The BitMessage component is part of the Bit.BlazorUI notification system and provides a flexible, accessible way to communicate status, errors, warnings, and important information to end users. It supports multiple severity levels, variants, sizes, and dismissal options with full customization capabilities.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | RenderFragment? | null | The content of the message |
| `Content` | RenderFragment? | null | Alias for ChildContent—the message text content |
| `Color` | BitColor? | null | General message color (Primary, Secondary, Tertiary, Info, Success, Warning, SevereWarning, Error, etc.) |
| `Variant` | BitVariant? | null | Message styling variant (Fill, Outline, Text) |
| `Size` | BitSize? | null | Message size (Small, Medium, Large) |
| `Alignment` | BitAlignment? | null | Determines alignment of message content section |
| `Multiline` | bool | false | Determines if the message is multi-lined |
| `Truncate` | bool | false | Determines if the message text is truncated with expand/collapse toggle |
| `HideIcon` | bool | false | Prevents rendering the message icon |
| `IconName` | string? | null | Custom icon replacing the default message icon |
| `DismissIcon` | string? | null | Custom icon replacing the dismiss button |
| `ExpandIcon` | string? | null | Custom icon for expand button in Truncate mode |
| `CollapseIcon` | string? | null | Custom icon for collapse button in Truncate mode |
| `Elevation` | int? | null | Determines the elevation/shadow of the message (scale from 1 to 24) |
| `OnDismiss` | EventCallback | null | Callback triggered when the dismiss button is clicked |
| `AutoDismissTime` | TimeSpan? | null | Enables the auto-dismiss feature and sets the time to automatically call the OnDismiss callback |
| `Actions` | RenderFragment? | null | The content of custom actions to show on the message |
| `Role` | string? | null | Custom role attribute for accessibility |
| `Classes` | BitMessageClassStyles? | null | Custom CSS classes for BitMessage parts |
| `Styles` | BitMessageClassStyles? | null | Custom CSS styles for BitMessage parts |

### Severity/Message Types

The Message component supports the following severity levels through the `Color` parameter:

- **Info** (default) - Informational messages
- **Success** - Confirmation and success messages
- **Warning** - Warning messages requiring attention
- **SevereWarning** - High-priority warnings
- **Error** - Error and failure messages

### Dismissible Option

The Message component supports dismissal through:

- **OnDismiss**: EventCallback that triggers when the dismiss button is clicked, allowing you to handle the dismissal programmatically
- **AutoDismissTime**: TimeSpan property that enables automatic dismissal after a specified duration

### Customizable Style Parts (BitMessageClassStyles)

The following parts of the Message component can be customized via the `Classes` and `Styles` parameters:

- `Root` - Root element container
- `RootContainer` - Root container wrapper
- `Container` - Main container
- `IconContainer` - Icon wrapper container
- `Icon` - Icon element
- `ContentContainer` - Content wrapper
- `ContentWrapper` - Content inner wrapper
- `Content` - Message content element
- `Actions` - Actions container
- `ExpanderButton` - Expand/collapse button
- `ExpanderIcon` - Expand/collapse icon
- `DismissButton` - Dismiss button
- `DismissIcon` - Dismiss icon

### Usage Examples

The documentation includes example implementations for:

- **Basic usage** - Simple message display
- **Color variants** - Info, Success, Warning, SevereWarning, and Error colored messages
- **Fill/Outline/Text variants** - Different visual styles
- **Alignment options** - Content positioning within the message
- **Elevation** - Shadow depth effects
- **Multiline content** - Messages spanning multiple lines
- **Truncate** - Truncated text with expand/collapse toggle
- **Dismissible messages** - Messages that can be dismissed by the user
- **Auto-dismiss** - Messages that automatically disappear after a set duration
- **Action buttons** - Custom action buttons within the message
- **Icon customization** - Custom icons for message, expand, collapse, and dismiss
- **RTL support** - Right-to-left language support
- **Size variations** - Small, Medium, and Large message sizes
- **Advanced styling** - Custom CSS class and style customization

---

**Source:** [Bit.BlazorUI Message Component Documentation](https://blazorui.bitplatform.dev/components/message)

---

## Persona

The BitPersona component is a visual representation control for displaying user information. It presents user images or initials along with optional name, role, and online status indicators, integrating seamlessly with PeoplePicker and Facepile controls.

### Component Description

BitPersona displays "the image that person has chosen to upload themselves" with optional metadata including name, role, and presence status. It provides flexible customization through multiple properties for avatar styling, text content, and interactive features.

### Parameters & Properties

#### Image & Avatar Options
| Property | Type | Description |
|----------|------|-------------|
| `ImageUrl` | string | URL to square aspect ratio image |
| `ImageInitials` | string | User initials displayed when no image exists |
| `ImageAlt` | string | Alt text for accessibility |
| `ShowInitialsUntilImageLoads` | bool | Renders initials while image loads |

#### Coin (Avatar Circle) Properties
| Property | Type | Description |
|----------|------|-------------|
| `CoinSize` | int | Custom pixel size |
| `CoinColor` | BitColor | Background color for initials |
| `CoinShape` | BitPersonaCoinShape | Avatar shape: Circular or Square |
| `CoinVariant` | BitPersonaCoinVariant | Styling variant: Fill, Outline, or Text |
| `CoinTemplate` | RenderFragment | Custom image template |

#### Text Content
| Property | Type | Description |
|----------|------|-------------|
| `PrimaryText` | string | Usually person's name |
| `SecondaryText` | string | Typically user role |
| `TertiaryText` | string | Status information (size72+ only) |
| `OptionalText` | string | Custom message (size100+ only) |

#### Presence & Status
| Property | Type | Description |
|----------|------|-------------|
| `Presence` | BitPersonaPresence | Presence status: None, Online, Offline, Away, Dnd, Busy, Blocked |
| `PresenceTitle` | string | Tooltip text for presence icon |
| `PresenceIcons` | Dictionary | Custom icon dictionary for presence states |

#### Action & Interaction
| Property | Type | Description |
|----------|------|-------------|
| `OnActionClick` | EventCallback | Callback for custom action button clicks |
| `OnImageClick` | EventCallback | Callback for image clicks |
| `ActionButtonTitle` | string | Tooltip text (default: "Edit image") |
| `ActionIconName` | string | Icon name for action button |

### Available Sizes

The component supports nine predefined sizes measured in pixels:
- `Size8`
- `Size24`
- `Size32`
- `Size40`
- `Size48`
- `Size56`
- `Size72` - Enables TertiaryText support
- `Size100` - Enables OptionalText support
- `Size120`

### CSS Customization

The `BitPersonaClassStyles` class provides granular control over styling for the following elements:
- `Root` - Main container
- `CoinContainer` - Avatar container
- `ImageContainer` - Image display area
- `Initials` - Initials text
- `ActionButton` - Action button styling
- `DetailsContainer` - Text content container
- `PrimaryTextContainer` - Primary text styling
- `SecondaryTextContainer` - Secondary text styling
- `TertiaryTextContainer` - Tertiary text styling
- `OptionalTextContainer` - Optional text styling

---

## SnackBar

The **SnackBar** component (also known as a toast notification) provides brief, temporary notifications within the Bit BlazorUI framework. It's used for displaying user-friendly messages that appear and dismiss automatically or through user interaction.

### Component Description

SnackBar is a lightweight notification component that displays brief messages to users. It supports multiple display positions, color themes, automatic dismissal, and both simple and complex content layouts. It's commonly used for confirmations, alerts, and informational messages.

### Parameters & Properties

#### BitSnackBar Component Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AutoDismiss` | `bool` | `false` | Enables automatic dismissal of the snackbar after a set duration |
| `AutoDismissTime` | `TimeSpan?` | `null` | Duration before auto-dismiss (typically 3 seconds) |
| `Position` | `BitSnackBarPosition?` | `null` | Display location on screen (see Position Options below) |
| `Multiline` | `bool` | `false` | Enables multi-line mode for title and body text |
| `Persistent` | `bool` | `false` | Prevents user dismissal and hides the dismiss button |
| `Color` | `BitColor` | - | Theme color selection for the snackbar |
| `DismissIconName` | `string?` | `null` | Custom icon name for the dismiss button |
| `BodyTemplate` | `RenderFragment` | `null` | Custom Razor template for rendering message content |
| `TitleTemplate` | `RenderFragment` | `null` | Custom Razor template for rendering title content |

#### BitSnackBarItem Properties

Each snackbar item has the following properties:

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `Guid` | Unique identifier for the snackbar item |
| `Title` | `string` | Header/title text of the notification |
| `Body` | `string?` | Message content or body of the notification |
| `Color` | `BitColor?` | Item-specific color override |
| `Persistent` | `bool` | Whether this item is non-dismissible by the user |

### Position Options

The snackbar can be positioned in 6 different locations:

- **Top Positions**: `TopStart`, `TopCenter`, `TopEnd`
- **Bottom Positions**: `BottomStart`, `BottomCenter`, `BottomEnd`

### Color Options

The following color themes are available:

- `Primary`, `Secondary`, `Tertiary`
- `Info`, `Success`, `Warning`, `SevereWarning`, `Error`
- `PrimaryBackground`, `SecondaryBackground`, `TertiaryBackground`
- `PrimaryForeground`, `SecondaryForeground`, `TertiaryForeground`
- `PrimaryBorder`, `SecondaryBorder`, `TertiaryBorder`

### Duration & Auto-Dismissal

- **Default Duration**: 3 seconds (when `AutoDismissTime` is not specified)
- **Custom Duration**: Set `AutoDismissTime` to a custom `TimeSpan`
- **Manual Dismissal**: Users can manually close non-persistent snackbars
- **Persistent Mode**: Set `Persistent="true"` to prevent automatic or user-initiated dismissal

### Public Methods

| Method | Description |
|--------|-------------|
| `Show()` | Display a custom configured snackbar |
| `Info()` | Show an info-themed notification |
| `Success()` | Show a success-themed notification |
| `Warning()` | Show a warning-themed notification |
| `Error()` | Show an error-themed notification |
| `Close()` | Dismiss a specific snackbar item by ID |

### Code Example

```csharp
@page "/snackbar-demo"
@inject BitSnackBar BitSnackBar

<div>
    <button class="btn btn-primary" @onclick="ShowBasicSnackBar">Show Basic Snackbar</button>
    <button class="btn btn-success" @onclick="ShowSuccessSnackBar">Show Success</button>
    <button class="btn btn-warning" @onclick="ShowWarningSnackBar">Show Warning</button>
    <button class="btn btn-danger" @onclick="ShowErrorSnackBar">Show Error</button>
    <button class="btn btn-info" @onclick="ShowMultilineSnackBar">Show Multiline</button>
</div>

@code {
    private async Task ShowBasicSnackBar()
    {
        await BitSnackBar.Show(
            title: "Notification",
            body: "This is a basic snackbar message",
            position: BitSnackBarPosition.BottomEnd,
            autoDismiss: true,
            autoDismissTime: TimeSpan.FromSeconds(5)
        );
    }

    private async Task ShowSuccessSnackBar()
    {
        await BitSnackBar.Success(
            title: "Success",
            body: "Operation completed successfully",
            position: BitSnackBarPosition.TopEnd
        );
    }

    private async Task ShowWarningSnackBar()
    {
        await BitSnackBar.Warning(
            title: "Warning",
            body: "Please review this important message",
            position: BitSnackBarPosition.TopCenter
        );
    }

    private async Task ShowErrorSnackBar()
    {
        await BitSnackBar.Error(
            title: "Error",
            body: "An error occurred during processing",
            position: BitSnackBarPosition.BottomStart
        );
    }

    private async Task ShowMultilineSnackBar()
    {
        await BitSnackBar.Show(
            title: "Multi-line Message",
            body: "This snackbar demonstrates multi-line content with more detailed information for the user",
            multiline: true,
            position: BitSnackBarPosition.BottomEnd,
            autoDismiss: true,
            autoDismissTime: TimeSpan.FromSeconds(7)
        );
    }
}
```

---

This documentation provides a complete overview of the Bit.BlazorUI SnackBar component with all parameters, position options, color themes, and practical usage examples for implementing toast notifications in your Blazor applications.

---

## Tag

### Overview
The BitTag component (also called Chip) provides a visual representation of an attribute, person, or asset within Blazor UI applications. It's a versatile component for displaying labeled information with optional icons and dismissal capabilities.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Text` | `string?` | `null` | The tag's text content |
| `ChildContent` | `RenderFragment?` | `null` | Custom child content for template customization |
| `Color` | `BitColor?` | `null` | Visual color scheme for the tag |
| `Size` | `BitSize?` | `null` | Size option (Small, Medium, Large) |
| `Variant` | `BitVariant?` | `null` | Visual style variant (Fill, Outline, Text) |
| `IconName` | `string?` | `null` | Icon display within the tag |
| `OnDismiss` | `EventCallback` | — | Callback handler for close/dismiss button |
| `OnClick` | `EventCallback` | — | Click event handler |
| `Reversed` | `bool` | `false` | Reverses content flow direction |

### Enumerations

**BitColor** - Color scheme options:
- Primary, Secondary, Tertiary
- Info, Success, Warning, SevereWarning, Error
- PrimaryBackground, SecondaryBackground, TertiaryBackground
- PrimaryForeground, SecondaryForeground, TertiaryForeground
- PrimaryBorder, SecondaryBorder, TertiaryBorder

**BitSize** - Sizing options:
- Small
- Medium
- Large

**BitVariant** - Visual style variants:
- Fill (default)
- Outline
- Text

### Styling & Customization

The BitTag component supports custom styling through **BitTagClassStyles** properties that allow customization of:
- Root element styling
- Text styling
- Icon styling
- Dismiss button styling
- Dismiss icon styling

CSS classes and inline styles can be overridden for fine-grained control.

### Features

- **Icon Support**: Display an icon within the tag using the `IconName` parameter
- **Dismiss/Removable**: Enable dismissal functionality via the `OnDismiss` event callback
- **Template Customization**: Use `ChildContent` to provide custom content beyond simple text
- **Accessibility**: Built-in support for `AriaLabel` attribute
- **Color & Size Options**: Flexible theming with multiple color and size combinations
- **Visual Variants**: Choose between Fill, Outline, and Text styling approaches

### Usage Examples

Basic tag with text:
```razor
<BitTag Text="Primary Tag" Color="BitColor.Primary" />
```

Tag with icon and dismissal:
```razor
<BitTag Text="Removable Tag" 
        IconName="Delete" 
        OnDismiss="@HandleDismiss" 
        Color="BitColor.Success" />
```

Custom styled tag:
```razor
<BitTag Text="Custom Styled" 
        Variant="BitVariant.Outline" 
        Size="BitSize.Large"
        Color="BitColor.Info" />
```

Tag with custom content:
```razor
<BitTag>
    <ChildContent>
        <strong>Custom Content</strong>
    </ChildContent>
</BitTag>
```

---

This documentation provides a complete reference for implementing the BitTag component in your Blazor Server application. The component integrates seamlessly with Bit.BlazorUI's design system and supports both simple text labels and complex custom content.

---

# Progress

## Loading

The **BitLoading** component provides native loading indicators with beautiful visual animations ready to use in any waiting scenario. It's part of the bit BlazorUI library's Progress components, offering 18 different loading animation styles with customizable colors, sizes, and labels.

### Loading Indicator Styles

The component supports the following loading animation types:

- **BitBarsLoading** - Animated horizontal bars
- **BitCircleLoading** - Rotating circle animation
- **BitDotsRingLoading** - Ring of animated dots
- **BitDualRingLoading** - Dual rotating rings
- **BitEllipsisLoading** - Animated ellipsis (three dots)
- **BitGridLoading** - Animated grid pattern
- **BitHeartLoading** - Heart-shaped animation
- **BitHourglassLoading** - Animated hourglass
- **BitRingLoading** - Single rotating ring
- **BitRippleLoading** - Ripple wave animation
- **BitRollerLoading** - Rolling animation
- **BitSpinnerLoading** - Classic spinner
- **BitXboxLoading** - Xbox-style animation
- **BitSlickBarsLoading** - Slick bar animation
- **BitBouncingDotsLoading** - Bouncing dots animation
- **BitRollingDashesLoading** - Rolling dashes animation
- **BitOrbitingDotsLoading** - Orbiting dots around center
- **BitRollingSquareLoading** - Rolling square animation

### Component Parameters

#### BitLoading Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Color` | `BitColor?` | `null` | General color of the loading component. Options: Primary, Secondary, Tertiary, Info, Success, Warning, SevereWarning, Error |
| `CustomColor` | `string?` | `null` | Custom CSS color value for the loading animation |
| `CustomSize` | `int?` | `null` | Custom size in pixels |
| `Label` | `string?` | `null` | Text content to display below/around the loading indicator |
| `LabelPosition` | `BitLabelPosition?` | `null` | Position of the label. Options: Top, End, Bottom, Start |
| `LabelTemplate` | `RenderFragment?` | `null` | Custom template for rendering the label content |
| `Size` | `BitSize?` | `null` | Predefined size option. Options: Small, Medium, Large |

#### Inherited BitComponentBase Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaLabel` | `string?` | `null` | Accessible label for assistive technologies (ARIA) |
| `Class` | `string?` | `null` | CSS class name(s) to apply to the component |
| `Dir` | `BitDir?` | `null` | Text directionality. Options: Ltr (Left-to-Right), Rtl (Right-to-Left), Auto |
| `HtmlAttributes` | `Dictionary<string, object>` | `new Dictionary` | Additional HTML attributes to apply to the component |
| `Id` | `string?` | `null` | Unique HTML identifier |
| `IsEnabled` | `bool` | `true` | Enable or disable the component |
| `Style` | `string?` | `null` | CSS style string to apply inline |
| `TabIndex` | `string?` | `null` | Keyboard navigation tab order |
| `Visibility` | `BitVisibility` | `Visible` | Visibility state. Options: Visible, Hidden, Collapsed |

### Enumerations

**BitColor:** `Primary`, `Secondary`, `Tertiary`, `Info`, `Success`, `Warning`, `SevereWarning`, `Error`

**BitLabelPosition:** `Top`, `End`, `Bottom`, `Start`

**BitSize:** `Small`, `Medium`, `Large`

**BitVisibility:** `Visible`, `Hidden`, `Collapsed`

**BitDir:** `Ltr`, `Rtl`, `Auto`

### Public Members

- **UniqueId** (`Guid`): A readonly unique identifier automatically assigned to each component instance at construction
- **RootElement** (`ElementReference`): Reference to the root HTML DOM element for advanced scenarios

### Usage Examples

#### Basic Loading Indicator
```csharp
<BitCircleLoading />
```

#### Loading with Color
```csharp
<BitCircleLoading Color="BitColor.Primary" />
```

#### Loading with Custom Size
```csharp
<BitCircleLoading Size="BitSize.Large" />
<BitCircleLoading CustomSize="50" />
```

#### Loading with Label
```csharp
<BitCircleLoading Label="Loading..." LabelPosition="BitLabelPosition.Bottom" />
```

#### Loading with Custom Color
```csharp
<BitCircleLoading CustomColor="#FF5733" />
```

#### Custom Label Template
```csharp
<BitCircleLoading>
    <LabelTemplate>
        <div>Custom Loading Content</div>
    </LabelTemplate>
</BitCircleLoading>
```

#### Different Animation Styles
```csharp
<BitBarsLoading />
<BitSpinnerLoading Color="BitColor.Success" />
<BitDotsRingLoading Size="BitSize.Medium" />
<BitHeartLoading CustomColor="#FF1493" />
```

---

## Progress

The BitProgress component displays the completion status of an operation with support for multiple visualization modes and extensive customization options.

### Component Description

BitProgress is a flexible progress indicator component that supports two primary variants:
- **ProgressIndicator & ProgressBar**: Linear horizontal progress visualization
- **Circular Progress**: Radial progress representation

The component supports both determinate (percentage-based) and indeterminate (indefinite) progress states, making it suitable for various UI scenarios from file uploads to loading operations.

### Core Parameters

#### Progress Value & Behavior

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Percent` | `double` | `0` | Completion percentage (0-100 range) |
| `Indeterminate` | `bool` | `false` | Enables indefinite progress animation without percentage value |
| `Circular` | `bool` | `false` | Displays progress in circular/radial format instead of linear bar |

#### Labeling & Display

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Label` | `string?` | `null` | Text label displayed above the progress bar |
| `LabelTemplate` | `RenderFragment?` | `null` | Custom Razor template for rendering the label section |
| `Description` | `string?` | `null` | Supplementary text describing the operation |
| `DescriptionTemplate` | `RenderFragment?` | `null` | Custom Razor template for rendering the description |
| `ShowPercentNumber` | `bool` | `false` | Displays the percentage value as a numeric indicator |
| `PercentNumberFormat` | `string` | `{0:F0}` | Format string for percentage display (default: whole number) |

#### Styling & Appearance

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Thickness` | `int` | `2` | Progress bar thickness in pixels |
| `Radius` | `int` | `6` | Circular progress radius (for circular mode) |
| `Color` | `string?` | `null` | Custom color override for the progress bar |

#### Accessibility

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AriaValueText` | `string?` | `null` | Screen reader announcement for the current progress status |

#### Inherited Base Parameters

From `BitComponentBase`:
- `AriaLabel` - Accessibility label
- `Class` - CSS class names
- `Dir` - Directionality (LTR/RTL support)
- `HtmlAttributes` - Additional HTML attributes
- `Id` - Element identifier
- `IsEnabled` - Component enabled state
- `Style` - Inline CSS styles
- `TabIndex` - Tab order
- `Visibility` - Visibility control

### Progress Bar Styles

BitProgress provides the `BitProgressClassStyles` property for comprehensive customization of the following elements:

- **Root**: Main container element
- **Label**: Label text section
- **PercentNumber**: Percentage value display
- **BarContainer**: Progress bar wrapper
- **Track**: Background/inactive progress area
- **Bar**: Active progress indicator
- **Description**: Description text section

### Indeterminate Mode

The `Indeterminate` property enables animated indefinite progress visualization when the completion percentage is unknown. When set to `true`, the progress bar displays a continuous animation without a specific percentage value, ideal for operations with unpredictable duration.

```csharp
<BitProgress Indeterminate="true" Label="Processing..." />
```

### Progress Modes & Features

**Linear Progress** (default)
```csharp
<BitProgress Percent="45" Label="Download" ShowPercentNumber="true" />
```

**Circular Progress**
```csharp
<BitProgress Percent="75" Circular="true" Radius="50" />
```

**Indeterminate Linear**
```csharp
<BitProgress Indeterminate="true" Description="Loading content..." />
```

**Indeterminate Circular**
```csharp
<BitProgress Indeterminate="true" Circular="true" />
```

**Custom Styled Progress**
```csharp
<BitProgress 
    Percent="60" 
    Label="Upload Progress"
    ShowPercentNumber="true"
    PercentNumberFormat="{0:F1}"
    Color="#0078d4"
    Thickness="4"
    AriaValueText="60 percent complete" />
```

**With Custom Labels**
```csharp
<BitProgress Percent="50" Label="Processing">
    <DescriptionTemplate>
        <span>Completed: 5 of 10 files</span>
    </DescriptionTemplate>
</BitProgress>
```

### Accessibility Features

- **AriaLabel**: Define semantic meaning for screen readers
- **AriaValueText**: Announce current progress status to assistive technologies
- **RTL Support**: Full right-to-left language directionality support
- **IsEnabled**: Control component interaction state

### Notes

- The component supports full right-to-left (RTL) language compatibility via the `Dir` parameter
- Percentage values are clamped to the 0-100 range
- The default percentage format displays whole numbers; use `PercentNumberFormat` for decimal precision
- Indeterminate mode ignores the `Percent` parameter and displays continuous animation
- Custom styling through `BitProgressClassStyles` enables theme-consistent appearance

---

## Shimmer

The **BitShimmer** component is a temporary animation placeholder used when a service call takes time to return data and you want to avoid blocking the rendering of the rest of the UI. It provides visual feedback to users while waiting for content to load.

### Component Description

BitShimmer displays an animated skeleton screen that automatically transitions to actual content once data is loaded. It's designed for improved user experience during asynchronous operations by providing visual continuity rather than blank space.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **Height** | `string?` | `null` | Specifies the height of the shimmer placeholder |
| **Width** | `string?` | `null` | Specifies the width of the shimmer placeholder |
| **Circle** | `bool` | `false` | When true, renders the shimmer as a circle instead of a rectangle |
| **Color** | `BitColor?` | `null` | Specifies the color of the animated shimmer part |
| **Background** | `BitColor?` | `null` | Specifies the background color of the shimmer container |
| **Pulse** | `bool` | `false` | When true, enables pulse animation mode (default is wave animation) |
| **Loaded** | `bool` | `false` | Controls the transition from shimmer to actual content; when true, displays child content |
| **Delay** | `int?` | `null` | Animation delay in milliseconds |
| **Duration** | `int?` | `null` | Animation duration in milliseconds |
| **Template** | `RenderFragment?` | `null` | Custom RenderFragment for a custom shimmer template |
| **ChildContent** / **Content** | `RenderFragment?` | `null` | Content displayed when the Loaded property becomes true |
| **Classes** | `BitShimmerClassStyles?` | `null` | Custom CSS classes for styling individual shimmer elements |
| **Styles** | `BitShimmerClassStyles?` | `null` | Custom inline CSS styles for styling individual shimmer elements |
| **AriaLabel** | `string?` | `null` | Accessibility label for the shimmer |
| **Class** | `string?` | `null` | Custom CSS class for the root element |
| **Dir** | `Dir?` | `null` | Text direction (LTR/RTL) |
| **Id** | `string?` | `null` | HTML element ID |
| **IsEnabled** | `bool?` | `null` | Enables or disables the component |
| **Style** | `string?` | `null` | Custom inline CSS styles for the root element |
| **TabIndex** | `int?` | `null` | Tab index for keyboard navigation |
| **Visibility** | `Visibility?` | `null` | Controls component visibility |

### Animation Options

- **Wave** (default): A flowing shimmer effect that moves across the placeholder
- **Pulse**: An opacity-based pulsing animation that fades in and out

### Customization Structure

The **BitShimmerClassStyles** object allows customization of four main elements:

- **Root**: The outermost container element
- **Content**: The content wrapper when loaded
- **ShimmerWrapper**: The wrapper around the shimmer animation
- **Shimmer**: The animated shimmer element itself

### Available Colors

BitShimmer supports the following color options:
- Primary, Secondary, Tertiary
- Info, Success, Warning, SevereWarning, Error
- Background/Foreground/Border variants (Primary, Secondary, Tertiary)

### Usage Patterns

#### Basic Skeleton Loading
```csharp
<BitShimmer Height="100px" Width="100%" />
```

#### With Automatic Content Transition
```csharp
<BitShimmer Loaded="@isDataLoaded" Height="100px" Width="100%">
    @if (isDataLoaded)
    {
        <div>@loadedContent</div>
    }
</BitShimmer>
```

#### Circular Placeholder (Avatar/Profile Picture)
```csharp
<BitShimmer Circle="true" Height="64px" Width="64px" Color="BitColor.Primary" />
```

#### With Custom Animation
```csharp
<BitShimmer 
    Pulse="true" 
    Height="200px" 
    Width="100%"
    Duration="1500"
    Delay="500" />
```

#### Nested Skeleton Loading Pattern
```csharp
<div>
    <BitShimmer Height="30px" Width="60%" />
    <BitShimmer Height="20px" Width="100%" />
    <BitShimmer Height="20px" Width="100%" />
</div>
```

#### Content Swap with Animation
```csharp
<BitShimmer 
    Loaded="@dataLoaded" 
    Height="auto" 
    Width="100%"
    Color="BitColor.Secondary"
    Background="BitColor.BackgroundPrimary">
    @if (dataLoaded)
    {
        <Card>
            <h3>@Model.Title</h3>
            <p>@Model.Description</p>
        </Card>
    }
</BitShimmer>
```

#### Custom Template
```csharp
<BitShimmer Template="@CustomShimmerTemplate" />

@code {
    private RenderFragment CustomShimmerTemplate => @<template>
        <div class="custom-skeleton">
            <BitShimmer Height="40px" Width="40px" Circle="true" />
            <div>
                <BitShimmer Height="20px" Width="80%" />
                <BitShimmer Height="15px" Width="60%" />
            </div>
        </div>
    </template>;
}
```

### Key Features

- **Automatic Content Transition**: Swaps from shimmer to actual content via the `Loaded` property
- **Flexible Animations**: Choose between wave and pulse animation styles
- **Responsive Dimensions**: Use percentage-based or pixel-based sizing
- **Shape Flexibility**: Support for both rectangular and circular shapes
- **Color Theming**: Integrates with Bit's color system for consistent styling
- **Accessibility**: Includes ARIA label support for screen readers
- **Custom Templates**: Allows complete customization through RenderFragment templates
- **Performance**: Lightweight placeholder prevents layout shift during content loading

### Best Practices

1. **Match Content Dimensions**: Set shimmer dimensions to match the expected content size to avoid layout shift
2. **Skeleton Screen Layout**: Stack multiple shimmers to create a complete skeleton layout matching your content structure
3. **Animation Timing**: Use appropriate duration and delay values for visual polish
4. **Color Contrast**: Ensure sufficient contrast between shimmer color and background for visibility
5. **Content Preloading**: Combine with data fetching logic to swap content as soon as data arrives

---

# Surfaces

## Accordion

### Component Description

The **BitAccordion** component allows users to show and hide sections of related content on a page. It provides an expandable/collapsible container with customizable headers and content areas. The component supports single accordion items or can be grouped to create accordion groups where multiple items can expand independently or in controlled scenarios.

### Parameters & Properties

The BitAccordion component includes the following parameters and properties:

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Title` | `string?` | `null` | Header text displayed in the accordion header |
| `Description` | `string?` | `null` | Subtitle or short descriptive text displayed below the title |
| `IsExpanded` | `bool` | `false` | Controls whether the accordion is currently expanded (open/closed) |
| `DefaultIsExpanded` | `bool?` | `null` | Initial expansion state when the component is first rendered |
| `NoBorder` | `bool` | `false` | When true, removes the border styling and adds background color |
| `ChildContent` | `RenderFragment?` | `null` | Body content rendered inside the accordion when expanded |
| `HeaderTemplate` | `RenderFragment<bool>?` | `null` | Custom header rendering template; receives boolean for expanded state |
| `Class` | `string?` | `null` | Custom CSS classes applied to root element |
| `Style` | `string?` | `null` | Inline CSS styles applied to root element |
| `Id` | `string?` | `null` | HTML id attribute |
| `AriaLabel` | `string?` | `null` | Accessibility label for screen readers |
| `TabIndex` | `int?` | `null` | Tab order index for keyboard navigation |
| `IsEnabled` | `bool` | `true` | Enables or disables the accordion |
| `Dir` | `Dir` | `Ltr` | Text directionality (Ltr, Rtl, Auto) for RTL language support |
| `Visibility` | `Visibility` | — | Controls visibility of the component |
| `HtmlAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes to apply to the root element |

### Event Callbacks

- **`OnClick`**: Triggered when the accordion header is clicked
- **`OnChange`**: Fires when the `IsExpanded` value changes

### Styling & Customization

The component supports CSS customization through the **BitAccordionClassStyles** object, allowing customization of:

- `Root` - Root container styling
- `Expanded` - Styling when accordion is expanded
- `Header` - Header section styling
- `HeaderContent` - Content within the header
- `Title` - Title text styling
- `Description` - Description text styling
- `ChevronDownIcon` - Chevron icon styling
- `ContentContainer` - Container for body content
- `Content` - Body content styling

### Expand/Collapse Behavior

- **Default Behavior**: Accordion sections are collapsed by default
- **Programmatic Control**: Use the `IsExpanded` property to control open/closed state
- **User Interaction**: Clicking the header toggles the expansion state
- **Independent Items**: Multiple accordions can be used independently; each maintains its own state
- **Controlled Groups**: Can be configured to expand only one accordion at a time through parent component logic
- **Smooth Transitions**: Supports smooth expand/collapse animations

### Key Features

- **Customizable Headers**: Use `HeaderTemplate` for custom header layouts
- **Accessibility**: Supports `AriaLabel` and `TabIndex` for keyboard navigation and screen reader compatibility
- **RTL Support**: Built-in right-to-left language support via the `Dir` property
- **Flexible Styling**: Optional borders via `NoBorder` property
- **Disabled State**: Can be disabled via `IsEnabled` property
- **Subtitle Support**: Optional description text for additional context

### Code Examples

**Basic Single Accordion:**
```razor
<BitAccordion Title="Section 1" IsExpanded="false">
    <p>This is the accordion content for section 1.</p>
</BitAccordion>
```

**Accordion with Description:**
```razor
<BitAccordion Title="Frequently Asked Questions" Description="Common questions about our service">
    <p>Answer to the question goes here.</p>
</BitAccordion>
```

**Controlled Accordion (Two-Way Binding):**
```razor
<BitAccordion Title="Settings" @bind-IsExpanded="isSettingsOpen">
    <p>Configuration options here.</p>
</BitAccordion>

@code {
    private bool isSettingsOpen = false;
}
```

**Multiple Accordions with Independent States:**
```razor
<BitAccordion Title="Section 1">
    <p>Content for section 1</p>
</BitAccordion>

<BitAccordion Title="Section 2">
    <p>Content for section 2</p>
</BitAccordion>

<BitAccordion Title="Section 3">
    <p>Content for section 3</p>
</BitAccordion>
```

**Custom Header Template:**
```razor
<BitAccordion @bind-IsExpanded="isExpanded">
    <HeaderTemplate Context="isOpen">
        <div>
            <span>@(isOpen ? "▼" : "▶")</span>
            <strong>Custom Header</strong>
        </div>
    </HeaderTemplate>
    <p>Custom header content</p>
</BitAccordion>

@code {
    private bool isExpanded = false;
}
```

**No Border Style:**
```razor
<BitAccordion Title="Simple Style" NoBorder="true">
    <p>Content without border styling</p>
</BitAccordion>
```

**With Event Handlers:**
```razor
<BitAccordion Title="Click Me" OnChange="HandleAccordionChange">
    <p>Content triggered by change event</p>
</BitAccordion>

@code {
    private void HandleAccordionChange(bool isExpanded)
    {
        Console.WriteLine($"Accordion expanded: {isExpanded}");
    }
}
```

**RTL Language Support:**
```razor
<BitAccordion Title="عنوان" Dir="Dir.Rtl" IsExpanded="false">
    <p>محتوى الأكورديون</p>
</BitAccordion>
```

---

## Callout

### Component Description
A callout is an anchored tip that can be used to teach people or guide them through the app without blocking them. It functions as a popover/popup element that displays contextual information anchored to a specific element on the page.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Anchor` | `RenderFragment?` | `null` | Content to render as the anchor element that triggers the callout |
| `AnchorEl` | `Func<ElementReference>?` | `null` | Function returning an external anchor element reference for positioning |
| `AnchorId` | `string?` | `null` | ID of an external anchor element to position the callout relative to |
| `ChildContent` / `Content` | `RenderFragment?` | `null` | The content displayed inside the callout |
| `IsOpen` | `bool` | `false` | Controls whether the callout is open or closed |
| `Direction` | `BitDropDirection?` | `null` | Allowed expansion directions (All or TopAndBottom) |
| `FixedCalloutWidth` | `bool` | `false` | Preserves the component width without automatic sizing |
| `SetCalloutWidth` | `bool` | `false` | Sets the callout width based on available space |
| `ScrollContainerId` | `string?` | `null` | ID of a scrollable container element for content overflow handling |
| `ResponsiveMode` | `BitResponsiveMode?` | `null` | Responsive behavior configuration |
| `MaxWindowWidth` | `int?` | `null` | Maximum width for position calculation |

### Direction Options

- **All**: Automatic direction selection based on available space (default positioning behavior)
- **TopAndBottom**: Restricts positioning to vertical directions only (top or bottom relative to anchor)

### Public Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| `Toggle()` | `Task` | Programmatically toggle the callout open/closed state |

### Styling & Customization

The component supports custom styling through `BitCalloutClassStyles` for the following elements:
- **Root**: Main callout container
- **AnchorContainer**: Container for the anchor element
- **Opened**: State-specific styling when callout is open
- **Content**: Inner content area
- **Overlay**: Background overlay element

### Usage Patterns

The component supports multiple anchoring approaches:

1. **Inline Anchor**: Define anchor content directly within the component
2. **External Anchor by ID**: Reference an existing element via `AnchorId`
3. **External Anchor by Reference**: Use `AnchorEl` with an `ElementReference`
4. **State Binding**: Bind `IsOpen` to a boolean variable for programmatic control
5. **Scrollable Content**: Use `ScrollContainerId` to enable scrolling within the callout

### Key Features

- **Automatic Positioning**: When `Direction="All"`, the component automatically positions itself based on available viewport space
- **Scrollable Content**: Support for scrollable content via `ScrollContainerId` parameter
- **Width Control**: Flexible width configuration with `FixedCalloutWidth` and `SetCalloutWidth` options
- **Responsive Design**: Built-in responsive behavior configuration
- **Accessible**: Designed as a non-blocking tip/guide mechanism for user education and app navigation

Sources:
- [Bit.BlazorUI Callout Component](https://blazorui.bitplatform.dev/components/callout)

---

## Card

The BitCard component functions as a container to wrap around specific content. Keeping a card to a single subject keeps the design clean.

### Properties

#### BitCard-Specific Properties

| Property | Type | Description |
|----------|------|-------------|
| **ChildContent** | RenderFragment | The inner content rendered within the card |
| **Background** | BitColorKind? | Sets the card's background color scheme |
| **Border** | BitColorKind? | Applies a border with specified color styling |
| **NoShadow** | bool | Disables the default shadow effect when set to true |
| **FullWidth** | bool | Stretches the card to 100% of parent width |
| **FullHeight** | bool | Stretches the card to 100% of parent height |
| **FullSize** | bool | Applies both full width and height |

#### Inherited Base Properties (From BitComponentBase)

| Property | Type | Description |
|----------|------|-------------|
| **Class** | string? | CSS class names |
| **Style** | string? | Inline CSS styling |
| **Id** | string? | Unique element identifier |
| **AriaLabel** | string? | Accessibility label for assistive technologies |
| **Dir** | BitDir? | Text directionality (Ltr, Rtl, Auto) |
| **TabIndex** | string? | Keyboard navigation order |
| **IsEnabled** | bool | Component interaction state |
| **Visibility** | BitVisibility | Display state (Visible, Hidden, Collapsed) |

### Color Options

The **Background** and **Border** properties support the following **BitColorKind** enum values:
- **Primary** - Primary color scheme
- **Secondary** - Secondary color scheme
- **Tertiary** - Tertiary color scheme
- **Transparent** - No color (transparent)

### Styling Variants

The Card component supports the following styling configurations:

- **Basic** - Default card with shadow effect
- **NoShadow** - Card without the default shadow
- **Background** - Card with custom background color
- **Border** - Card with colored border
- **FullSize** - Card stretched to 100% width and height
- **FullWidth** - Card stretched to 100% width
- **FullHeight** - Card stretched to 100% height

### Basic Usage Example

```csharp
<BitCard>
    <p>Your content goes here</p>
</BitCard>
```

### Card with No Shadow

```csharp
<BitCard NoShadow="true">
    <p>Content without shadow</p>
</BitCard>
```

### Card with Background Color

```csharp
<BitCard Background="BitColorKind.Primary">
    <p>Card with primary background</p>
</BitCard>
```

### Card with Border

```csharp
<BitCard Border="BitColorKind.Secondary">
    <p>Card with secondary border</p>
</BitCard>
```

### Card with Full Size

```csharp
<BitCard FullSize="true">
    <p>Full width and height card</p>
</BitCard>
```

---

## Collapse

The Collapse component enables users to toggle the visibility of related content sections. It allows the user to show and hide sections of related content on a page with customizable styling and animation behavior.

### Component Description

The **BitCollapse** component provides a simple mechanism to expand and collapse content sections. It supports custom styling, RTL (right-to-left) language support, full accessibility attributes, and responsive design suitable for all modern browsers including iOS Safari.

### Parameters & Properties

#### BitCollapse-Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Expanded` | `bool` | `false` | Controls the initial state of the collapse component (expanded or collapsed) |
| `ChildContent` | `RenderFragment?` | `null` | The collapsible content that will be shown/hidden based on the Expanded state |
| `Classes` | `BitCollapseClassStyles?` | `null` | Custom CSS classes for different component parts (Root, Expanded, Content) |
| `Styles` | `BitCollapseClassStyles?` | `null` | Custom CSS styles for different component parts (Root, Expanded, Content) |

#### Inherited Parameters (from BitComponentBase)

| Parameter | Type | Description |
|-----------|------|-------------|
| `AriaLabel` | `string?` | ARIA label for accessibility |
| `Class` | `string?` | CSS class attribute |
| `Dir` | `string?` | Text direction (supports RTL via "rtl" value) |
| `Id` | `string?` | HTML element ID |
| `IsEnabled` | `bool` | Enable/disable the component |
| `Style` | `string?` | Inline CSS styles |
| `TabIndex` | `int?` | Tab order index |
| `Visibility` | `BitVisibility` | Control visibility of the component |
| `HtmlAttributes` | `Dictionary<string, object>?` | Additional HTML attributes |

### BitCollapseClassStyles Properties

The `BitCollapseClassStyles` class provides granular control over component styling:

| Property | Purpose |
|----------|---------|
| `Root` | Applies CSS classes/styles to the root container element |
| `Expanded` | Applies CSS classes/styles specifically when the component is in expanded state |
| `Content` | Applies CSS classes/styles to the content area |

### Expand/Collapse Animation

The component supports toggle functionality through the `Expanded` boolean parameter. The animation behavior is driven by CSS transitions and can be customized through the `Classes` and `Styles` properties. Content visibility is controlled by binding the `Expanded` parameter to a boolean state variable.

### Code Examples

#### Basic Usage

```csharp
<BitCollapse Expanded="true">
    <p>This content can be collapsed and expanded.</p>
</BitCollapse>
```

#### With State Control

```csharp
@page "/collapse-example"

<button @onclick="ToggleCollapse">Toggle Collapse</button>

<BitCollapse Expanded="@isExpanded">
    <p>This content toggles visibility based on the isExpanded state.</p>
</BitCollapse>

@code {
    private bool isExpanded = false;

    private void ToggleCollapse()
    {
        isExpanded = !isExpanded;
    }
}
```

#### Style & Class Customization

```csharp
<BitCollapse Expanded="true"
             Classes="new BitCollapseClassStyles { 
                 Root = 'custom-collapse-root',
                 Expanded = 'custom-expanded-state',
                 Content = 'custom-content'
             }">
    <p>Styled collapse component</p>
</BitCollapse>
```

#### RTL (Right-to-Left) Support

```csharp
<BitCollapse Expanded="true" Dir="rtl">
    <p>محتوى قابل للطي</p>
</BitCollapse>
```

#### Accessibility Example

```csharp
<BitCollapse Expanded="true" 
             AriaLabel="Expandable section for additional information"
             Id="collapsible-section">
    <p>Accessible collapse component with proper ARIA labeling.</p>
</BitCollapse>
```

### Key Features

- **Toggle Visibility**: Simple boolean binding for expand/collapse control
- **Custom Styling**: Full support for custom CSS classes and inline styles via `BitCollapseClassStyles`
- **Accessibility**: Built-in support for ARIA labels and standard accessibility attributes
- **RTL Support**: Complete right-to-left language support via the `Dir` parameter
- **Responsive Design**: Works seamlessly across all modern browsers and devices
- **iOS Safari Compatible**: Includes auto-zoom prevention for mobile compatibility

---

## Dialog

A Blazor UI component that displays temporary pop-ups requiring user interaction. Dialogs take focus from the page or app and require people to interact with them before returning to the main content.

### Description

The **BitDialog** component is a versatile modal/modeless dialog component that supports various positioning options, modal blocking, dragging, and extensive customization. It provides a structured way to present information or gather user input with configurable buttons and event handling.

### Parameters & Properties

#### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls whether the dialog is visible and active |
| `Title` | `string?` | `null` | Header text displayed in the dialog title bar |
| `Message` | `string?` | `null` | Main content/body text of the dialog |
| `IsBlocking` | `bool` | `false` | When true, prevents dismissal by clicking outside the dialog |
| `IsModeless` | `bool` | `false` | When true, creates a non-modal dialog without overlay |
| `IsDraggable` | `bool` | `false` | When true, enables drag functionality for the dialog |
| `Position` | `BitDialogPosition` | `Center` | Controls where the dialog appears on screen |
| `AutoToggleScroll` | `bool` | `false` | Automatically manages main content scrollbar visibility |
| `AbsolutePosition` | `bool` | `false` | Uses absolute positioning instead of fixed positioning |

#### Button Configuration

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ShowOkButton` | `bool` | `true` | Display the confirmation/OK button |
| `ShowCancelButton` | `bool` | `true` | Display the cancellation button |
| `ShowCloseButton` | `bool` | `true` | Display the close icon button |
| `OkText` | `string` | `"Ok"` | Label text for the confirmation button |
| `CancelText` | `string` | `"Cancel"` | Label text for the cancel button |

#### Customization Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| `DragElementSelector` | `string?` | CSS selector for the drag handle element |
| `ScrollerSelector` | `string?` | CSS selector for the scrollable element (default: `body`) |
| `TitleAriaId` | `string?` | ARIA ID for dialog title labeling |
| `SubtitleAriaId` | `string?` | ARIA ID for dialog subtitle labeling |
| `IsAlert` | `bool` | When true, applies `alertdialog` ARIA role; when false, uses `dialog` role |

#### Style & Class Parameters

- **Root**: Styles applied to the root dialog container
- **Overlay**: Styles for the backdrop overlay
- **Container**: Inner container styling
- **Header**: Dialog header area styling
- **Body**: Dialog body/content area styling
- **Title**: Title text styling
- **Message**: Message text styling
- **ButtonsContainer**: Footer buttons container styling
- **OkButton**: Confirmation button styling
- **CancelButton**: Cancel button styling
- **Spinner**: Loading indicator styling

### Dialog Position Options

The `BitDialogPosition` enum provides nine positioning options:

- `Center` - Centered on screen (default)
- `TopLeft` - Upper left corner
- `TopCenter` - Top center
- `TopRight` - Upper right corner
- `CenterLeft` - Middle left
- `CenterRight` - Middle right
- `BottomLeft` - Lower left corner
- `BottomCenter` - Bottom center
- `BottomRight` - Lower right corner

### Modal Dialog Options

#### Blocking Dialogs
Set `IsBlocking="true"` to prevent users from dismissing the dialog by clicking outside it. Users must interact with the buttons or the close icon.

#### Modeless Dialogs
Set `IsModeless="true"` to create a non-modal dialog that doesn't overlay or block interaction with the page content. Modeless dialogs don't display an overlay backdrop.

#### Draggable Dialogs
Set `IsDraggable="true"` to allow users to drag the dialog around the screen. Optionally specify `DragElementSelector` to designate a specific element as the drag handle.

### Open/Close Handling

#### State Binding

Bind the `IsOpen` parameter to control dialog visibility:

```csharp
@page "/dialog-example"
@using Bit.BlazorUI

<BitButton OnClick="() => isOpen = true">Open Dialog</BitButton>

<BitDialog @bind-IsOpen="isOpen" 
           Title="Confirmation"
           Message="Are you sure?">
</BitDialog>

@code {
    private bool isOpen = false;
}
```

#### Event Handlers

| Event | Trigger | Usage |
|-------|---------|-------|
| `IsOpenChanged` | Dialog open/close state changes | Respond to visibility state changes |
| `OnOk` | OK/confirmation button clicked | Handle affirmative user action |
| `OnCancel` | Cancel button clicked | Handle cancellation |
| `OnClose` | Close icon clicked | Handle close icon interaction |
| `OnDismiss` | Dialog closes (any method) | Perform cleanup or final actions |

#### Event Handler Example

```csharp
<BitDialog @bind-IsOpen="isOpen"
           Title="Save Changes"
           Message="Do you want to save before closing?"
           OnOk="HandleOk"
           OnCancel="HandleCancel"
           OnClose="HandleClose">
</BitDialog>

@code {
    private bool isOpen = false;

    private async Task HandleOk()
    {
        // Handle OK action
        await SaveChanges();
        isOpen = false;
    }

    private void HandleCancel()
    {
        // Handle cancellation
        isOpen = false;
    }

    private void HandleClose()
    {
        // Handle close button click
        isOpen = false;
    }
}
```

### Code Examples

#### Basic Dialog

```csharp
<BitDialog @bind-IsOpen="isOpen"
           Title="Hello"
           Message="Welcome to the dialog component!">
</BitDialog>

<BitButton OnClick="() => isOpen = true">Show Dialog</BitButton>

@code {
    private bool isOpen = false;
}
```

#### Confirmation Dialog with Custom Buttons

```csharp
<BitDialog @bind-IsOpen="isConfirmOpen"
           Title="Delete Item"
           Message="This action cannot be undone. Continue?"
           OkText="Delete"
           CancelText="Keep It"
           IsBlocking="true"
           OnOk="HandleDelete"
           OnCancel="() => isConfirmOpen = false">
</BitDialog>

@code {
    private bool isConfirmOpen = false;

    private async Task HandleDelete()
    {
        // Perform deletion
        await DeleteItemAsync();
        isConfirmOpen = false;
    }
}
```

#### Positioned Dialog with Dragging

```csharp
<BitDialog @bind-IsOpen="isDragOpen"
           Title="Draggable Dialog"
           Message="You can drag this dialog around"
           Position="BitDialogPosition.TopRight"
           IsDraggable="true"
           DragElementSelector=".bit-dialog-header">
</BitDialog>

<BitButton OnClick="() => isDragOpen = true">Open Draggable Dialog</BitButton>

@code {
    private bool isDragOpen = false;
}
```

#### Alert Dialog (Accessibility-Focused)

```csharp
<BitDialog @bind-IsOpen="isAlertOpen"
           Title="Important Notice"
           Message="This is an important message"
           IsAlert="true"
           TitleAriaId="alert-title"
           SubtitleAriaId="alert-message"
           ShowCancelButton="false">
</BitDialog>

@code {
    private bool isAlertOpen = false;
}
```

#### Modeless Dialog

```csharp
<BitDialog @bind-IsOpen="isModelessOpen"
           Title="Modeless Dialog"
           Message="This dialog doesn't block the page"
           IsModeless="true"
           Position="BitDialogPosition.BottomRight">
</BitDialog>

@code {
    private bool isModelessOpen = false;
}
```

### Accessibility

The BitDialog component supports accessibility through ARIA attributes:

- **`IsAlert`**: When true, applies the `alertdialog` ARIA role; when false, uses the standard `dialog` role
- **`TitleAriaId`**: Associates the dialog title element with an ARIA ID for proper labeling
- **`SubtitleAriaId`**: Associates subtitle or descriptive content with an ARIA ID

---

This comprehensive documentation covers all aspects of the BitDialog component and should integrate well with your Blazor workstation project documentation.

---

## Modal

### Component Description

Modals are temporary pop-ups that take focus from the page or app and require people to interact with them. Unlike a Dialog, a modal should be used for hosting lengthy content, such as privacy statements or license agreements, or for asking people to perform complex or multiple actions, such as changing settings.

### Parameters & Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls modal visibility |
| `Position` | `BitPosition?` | `null` | Sets modal location on screen |
| `Blocking` | `bool` | `false` | Prevents light dismissal by overlay click |
| `Modeless` | `bool` | `false` | Makes modal non-modal without overlay |
| `Draggable` | `bool` | `false` | Enables drag functionality |
| `FullSize` | `bool` | `false` | Makes modal full screen size |
| `FullWidth` | `bool` | `false` | Makes modal full width |
| `FullHeight` | `bool` | `false` | Makes modal full height |
| `AutoToggleScroll` | `bool` | `false` | Auto scrollbar toggle behavior |
| `AbsolutePosition` | `bool` | `false` | Uses absolute vs. fixed positioning |
| `ChildContent` | `RenderFragment?` | `null` | Modal content area |
| `Classes` | `BitModalClassStyles?` | `null` | Custom CSS classes for Root, Overlay, Content |
| `Styles` | `BitModalClassStyles?` | `null` | Custom CSS styles for Root, Overlay, Content |

### Events

- **`OnDismiss`** (`EventCallback<MouseEventArgs>`) - Fired when modal is dismissed
- **`OnOverlayClick`** (`EventCallback<MouseEventArgs>`) - Fired when overlay is clicked

### Modal Overlay Options

The modal overlay behavior is controlled through two key properties:

- **`Blocking`**: When set to `true`, the overlay prevents dismissal by clicking outside the modal, making it a blocking modal that requires user interaction to close
- **`Modeless`**: When set to `true`, no overlay appears, creating a modeless modal that doesn't dim the background

### Position Enum Values

The `BitPosition` enum provides the following positioning options:

- `TopLeft`, `TopCenter`, `TopRight`, `TopStart`, `TopEnd`
- `CenterLeft`, `Center`, `CenterRight`, `CenterStart`, `CenterEnd`
- `BottomLeft`, `BottomCenter`, `BottomRight`, `BottomStart`, `BottomEnd`

### Usage Example

```blazor
<BitModal @bind-IsOpen="isModalOpen" 
          Position="BitPosition.Center"
          Blocking="false"
          OnDismiss="@((args) => HandleDismiss())">
    <div class="modal-header">
        <h2>Modal Title</h2>
    </div>
    <div class="modal-body">
        <!-- Your content here -->
    </div>
    <div class="modal-footer">
        <button @onclick="@(() => isModalOpen = false)">Close</button>
    </div>
</BitModal>
```

This documentation provides all essential information for implementing the Bit.BlazorUI Modal component in your Blazor applications.

---

## Panel

### Description
Panels are overlays that contain supplementary content and are used for complex creation, edit, or management experiences. They display details about list items or manage settings through slide-out overlays.

### Component Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `IsOpen` | `bool` | `false` | Controls panel visibility state |
| `Position` | `BitPanelPosition?` | `null` | Determines panel placement on screen (Start, End, Top, Bottom) |
| `Size` | `double?` | `null` | Sets height or width based on position |
| `Blocking` | `bool` | `false` | Prevents panel dismissal by clicking the overlay |
| `Modeless` | `bool` | `false` | Removes the overlay element entirely when true |
| `AutoToggleScroll` | `bool` | `false` | Enables automatic scrollbar toggle behavior |
| `ChildContent` | `RenderFragment?` | `null` | Contains the panel content markup |

### Position Options

The `Position` parameter supports four positioning values via the `BitPanelPosition` enum:

- **Start** (0) - Positions panel on the left side
- **End** (1) - Positions panel on the right side
- **Top** (2) - Positions panel in the upper area
- **Bottom** (3) - Positions panel in the lower area

### Styling & Customization

Custom styling is available through the `BitPanelClassStyles` property, which allows customization of:

- **Root** - Main panel element styling
- **Overlay** - Background overlay styling
- **Container** - Content wrapper styling

### Interaction Features

| Feature | Type | Description |
|---------|------|-------------|
| `OnDismiss` | `EventCallback` | Callback invoked when the panel closes |
| `SwipeTrigger` | `double` | Swipe gesture threshold (default: 0.25) |
| `OnSwipeStart` | `EventCallback` | Callback triggered when swipe gesture starts |
| `OnSwipeMove` | `EventCallback` | Callback triggered during swipe movement |
| `OnSwipeEnd` | `EventCallback` | Callback triggered when swipe gesture ends |
| `ScrollerSelector` | `string` | CSS selector for target element to manage scroll behavior |

### Inherited Parameters

The BitPanel component inherits parameters from `BitComponentBase`, including:

- `AriaLabel` - Accessibility label
- `Class` - Additional CSS classes
- `Dir` - Text direction (LTR/RTL)
- `HtmlAttributes` - Custom HTML attributes
- `Id` - Element identifier
- `IsEnabled` - Enable/disable state
- `Style` - Inline CSS styles
- `TabIndex` - Tab order
- `Visibility` - Visibility control

### Basic Usage Example

```razor
<BitPanel @bind-IsOpen="isPanelOpen" Position="BitPanelPosition.End" Size="400">
    <h2>Panel Title</h2>
    <p>Your panel content goes here</p>
</BitPanel>

@code {
    bool isPanelOpen = false;
}
```

### Advanced Example with Callbacks

```razor
<BitPanel 
    @bind-IsOpen="isPanelOpen" 
    Position="BitPanelPosition.Start"
    Size="350"
    OnDismiss="HandlePanelDismiss"
    Blocking="false"
    AutoToggleScroll="true">
    
    <div class="panel-header">
        <h3>Settings</h3>
    </div>
    
    <div class="panel-content">
        <p>Configure your preferences here</p>
    </div>
</BitPanel>

@code {
    bool isPanelOpen = false;

    private async Task HandlePanelDismiss()
    {
        isPanelOpen = false;
        // Handle panel close logic
    }
}
```

### Swipe Gesture Example

```razor
<BitPanel 
    @bind-IsOpen="isPanelOpen"
    Position="BitPanelPosition.End"
    SwipeTrigger="0.25"
    OnSwipeEnd="HandleSwipeEnd">
    
    <p>Swipe to dismiss this panel</p>
</BitPanel>

@code {
    bool isPanelOpen = false;

    private async Task HandleSwipeEnd()
    {
        isPanelOpen = false;
    }
}
```

Sources:
- [Bit.BlazorUI Panel Component](https://blazorui.bitplatform.dev/components/panel)

---

## ScrollablePane

### Description
The ScrollablePane (also called ScrollView) is a component for scrolling through content that doesn't fit entirely on the screen. It's part of the Surfaces component category in Bit BlazorUI and provides flexible scrolling behavior with customizable dimensions and scrollbar styling.

### Parameters & Properties

| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `AutoScroll` | `bool` | `false` | Automatically scrolls to end after each render |
| `AutoHeight` | `bool` | `false` | Makes height automatic |
| `AutoWidth` | `bool` | `false` | Makes width automatic |
| `AutoSize` | `bool` | `false` | Makes both width and height automatic |
| `Height` | `double?` | `null` | Sets pane height in pixels |
| `Width` | `double?` | `null` | Sets pane width in pixels |
| `Modern` | `bool` | `false` | Enables modern scrollbar styling |
| `Overflow` | `BitOverflow?` | `null` | Controls overall scrollbar visibility |
| `OverflowX` | `BitOverflow?` | `null` | Controls horizontal scrollbar visibility |
| `OverflowY` | `BitOverflow?` | `null` | Controls vertical scrollbar visibility |
| `ScrollbarColor` | `string?` | `null` | Sets scrollbar track and thumb colors |
| `ScrollbarWidth` | `BitScrollbarWidth?` | `null` | Controls scrollbar thickness |
| `Gutter` | `BitScrollbarGutter?` | `null` | Reserves space for scrollbar to prevent layout shift |
| `ChildContent` | `RenderFragment?` | `null` | Content to render within the scrollable pane |

### Scroll Behavior

The ScrollablePane component provides programmatic scroll control through the `ScrollToEnd()` method, which scrolls the pane to the end of its content both horizontally and vertically.

### Overflow Options

The `Overflow`, `OverflowX`, and `OverflowY` parameters accept the following values from the `BitOverflow` enum:

- **`Auto`**: Displays scrollbars only when content overflows
- **`Hidden`**: Always hides scrollbars (content remains scrollable)
- **`Scroll`**: Always displays scrollbars whether needed or not
- **`Visible`**: Allows overflow to display outside the padding box

### Scrollbar Width Options

The `ScrollbarWidth` parameter accepts values from the `BitScrollbarWidth` enum:

- **`Auto`**: Uses platform/browser default scrollbar width
- **`Thin`**: Displays a thinner variant of the scrollbar
- **`None`**: Hides the scrollbar (content remains scrollable via mouse wheel or keyboard)

### Code Example

```csharp
<BitScrollablePane Height="300" Width="400" AutoScroll="false" OverflowY="BitOverflow.Auto">
    <div>
        <!-- Your scrollable content goes here -->
        <p>Long content that requires scrolling...</p>
    </div>
</BitScrollablePane>
```

### Key Features

- **Flexible sizing**: Control dimensions manually or use automatic sizing options
- **Customizable scrollbars**: Modern styling, color customization, and width control
- **Layout-aware**: The `Gutter` property can prevent layout shift when scrollbars appear/disappear
- **Programmatic control**: Use `ScrollToEnd()` for automatic scroll-to-bottom functionality
- **Responsive overflow handling**: Independent control over horizontal and vertical scrolling

---

## Splitter

### Description

The Splitter component divides a container into two adjustable sections, either horizontally or vertically. Users can resize these sections by dragging the divider.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **GutterSize** | int? | null | Width/height of the divider (gutter) in pixels |
| **GutterIcon** | string? | null | Icon displayed on the divider |
| **FirstPanel** | RenderFragment? | null | Content for the initial/left panel |
| **SecondPanel** | RenderFragment? | null | Content for the final/right panel |
| **Vertical** | bool | false | Switches orientation to vertical layout (stacks panels top/bottom instead of left/right) |

### Panel Sizing

Each panel supports individual sizing control through the following parameters:

**FirstPanel Sizing:**
- **FirstPanelSize** — Sets the initial dimension for the first panel
- **FirstPanelMinSize** — Establishes the minimum resizable boundary for the first panel
- **FirstPanelMaxSize** — Establishes the maximum resizable boundary for the first panel

**SecondPanel Sizing:**
- **SecondPanelSize** — Sets the initial dimension for the second panel
- **SecondPanelMinSize** — Establishes the minimum resizable boundary for the second panel
- **SecondPanelMaxSize** — Establishes the maximum resizable boundary for the second panel

### Orientation

- **Horizontal (default)**: Divides the container left-to-right with a vertical gutter
- **Vertical**: Divides the container top-to-bottom with a horizontal gutter
  - Set `Vertical="true"` to enable vertical orientation

### Inherited Parameters (BitComponentBase)

The Splitter inherits common component parameters:

| Parameter | Type | Description |
|-----------|------|-------------|
| **AriaLabel** | string? | Accessible label for assistive technologies |
| **Class** | string? | CSS class names for styling |
| **Dir** | Direction? | Text directionality (Ltr, Rtl, Auto) |
| **Id** | string? | Unique identifier |
| **IsEnabled** | bool? | Enables/disables user interaction |
| **Style** | string? | Inline CSS styles |
| **TabIndex** | int? | Keyboard navigation order |
| **Visibility** | Visibility? | Display state (Visible, Hidden, Collapsed) |

### Implementation Patterns

The component supports six primary use cases:

1. **Basic Horizontal Split** — Standard left-right division with default settings
2. **Vertical Orientation** — Top-bottom division using `Vertical="true"`
3. **Panel Size Configuration** — Custom initial dimensions and constraints
4. **Nested Splitters** — Multiple splitters for complex layouts
5. **Gutter Size Customization** — Adjust divider dimensions with `GutterSize`
6. **Gutter Icon** — Add visual indicators on the divider with `GutterIcon`

---

This documentation provides a comprehensive reference for implementing the Bit.BlazorUI Splitter component in your Blazor applications, with support for flexible layouts and responsive panel sizing.

---

## Tooltip

The **BitTooltip** component briefly describes unlabeled controls or provides additional information about labeled controls. It displays contextual information when users interact with an element.

### Component Description

BitTooltip is a lightweight component that wraps an anchor element (typically a button, icon, or text) and displays a tooltip with additional information. The tooltip can be triggered on hover, click, or focus, and positions itself intelligently around the anchor element.

### Parameters & Properties

#### Core Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Anchor` / `ChildContent` | `RenderFragment` | - | The element or control that the tooltip applies to |
| `Text` | `string` | - | The tooltip text content to display |
| `Template` | `RenderFragment` | - | Custom RenderFragment for tooltip content (alternative to Text) |
| `Position` | `BitTooltipPosition` | `Top` | The position where the tooltip appears relative to the anchor |
| `IsShown` | `bool` | - | Controls tooltip visibility state |
| `DefaultIsShown` | `bool` | `false` | Initial visibility state of the tooltip |
| `HideArrow` | `bool` | `false` | When true, removes the arrow indicator from the tooltip |

#### Delay Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowDelay` | `int` | - | Milliseconds to wait before showing the tooltip |
| `HideDelay` | `int` | - | Milliseconds to wait before hiding the tooltip |

#### Trigger Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ShowOnHover` | `bool` | `true` | Shows tooltip when the anchor element is hovered |
| `ShowOnClick` | `bool` | `false` | Shows tooltip when the anchor element is clicked |
| `ShowOnFocus` | `bool` | `false` | Shows tooltip when the anchor element receives focus |

### Positioning Options

The `BitTooltipPosition` enum provides 12 positioning options around the anchor element:

- `Top` - Above the anchor, centered
- `TopLeft` - Above the anchor, aligned to the left
- `TopRight` - Above the anchor, aligned to the right
- `RightTop` - To the right of the anchor, aligned to the top
- `Right` - To the right of the anchor, centered
- `RightBottom` - To the right of the anchor, aligned to the bottom
- `BottomRight` - Below the anchor, aligned to the right
- `Bottom` - Below the anchor, centered
- `BottomLeft` - Below the anchor, aligned to the left
- `LeftBottom` - To the left of the anchor, aligned to the bottom
- `Left` - To the left of the anchor, centered
- `LeftTop` - To the left of the anchor, aligned to the top

### Styling

#### Class Styles (`BitTooltipClassStyles`)

| Class Property | Purpose |
|----------------|---------|
| `Root` | Root container class for the tooltip component |
| `TooltipWrapper` | Wrapper around the tooltip content |
| `Tooltip` | Main tooltip element |
| `Arrow` | Arrow indicator pointing to the anchor |

#### Style Properties (`BitTooltipClassStyles`)

Custom CSS styling can be applied to the same parts as above through inline styles.

### Inherited Properties

As a `BitComponentBase`-derived component, BitTooltip inherits the following standard properties:

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | HTML id attribute |
| `Class` | `string` | CSS class(es) to apply |
| `Style` | `string` | Inline CSS styles |
| `AriaLabel` | `string` | ARIA label for accessibility |
| `IsEnabled` | `bool` | Enable/disable the component |
| `TabIndex` | `int` | Tab order index |
| `Dir` | `string` | Text direction (ltr/rtl) |
| `Visibility` | `BitVisibility` | Component visibility control |
| `HtmlAttributes` | `Dictionary<string, object>` | Additional HTML attributes |

### Code Examples

#### Basic Tooltip with Text

```razor
<BitTooltip Text="Click to refresh">
    <BitButton>Refresh</BitButton>
</BitTooltip>
```

#### Tooltip with Custom Position

```razor
<BitTooltip Text="Help information" Position="BitTooltipPosition.Right">
    <BitIconButton Icon="info"></BitIconButton>
</BitTooltip>
```

#### Tooltip with Delays

```razor
<BitTooltip Text="Additional details" ShowDelay="500" HideDelay="200">
    <BitButton>More Info</BitButton>
</BitTooltip>
```

#### Tooltip Triggered by Click

```razor
<BitTooltip Text="Copied to clipboard!" ShowOnClick="true" ShowOnHover="false">
    <BitButton>Copy</BitButton>
</BitTooltip>
```

#### Tooltip with Custom Content

```razor
<BitTooltip>
    <Template>
        <div class="custom-tooltip">
            <strong>Important:</strong>
            <p>This is custom HTML content</p>
        </div>
    </Template>
    <BitButton>Custom Tooltip</BitButton>
</BitTooltip>
```

#### Tooltip Without Arrow

```razor
<BitTooltip Text="Simple tooltip" HideArrow="true">
    <span>Hover me</span>
</BitTooltip>
```

#### Controlled Tooltip Visibility

```razor
@page "/tooltip-demo"

<BitTooltip @bind-IsShown="isTooltipShown" Text="This is controlled">
    <BitButton @onclick="ToggleTooltip">Toggle Tooltip</BitButton>
</BitTooltip>

@code {
    private bool isTooltipShown = false;

    private void ToggleTooltip()
    {
        isTooltipShown = !isTooltipShown;
    }
}
```

---

This documentation provides a complete reference for implementing the Bit.BlazorUI Tooltip component in your Blazor applications.

---

# Utilities

## Element

### Component Description

The **BitElement** component is a simple, flexible component with a customizable HTML tag that offers full control over styling, attributes, and directional flow. It serves as a generic wrapper component that can render as any standard HTML element (div, a, button, input, textarea, progress, etc.), making it useful for creating custom container elements with Bit styling and accessibility features built-in.

### Parameters & Properties

#### BitElement-Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Element` | `string?` | `"div"` | The custom HTML element tag used for the root node. Supports: div, a, button, input, textarea, progress, and other standard HTML elements. |
| `ChildContent` | `RenderFragment?` | `null` | The content to be rendered within the element. |

#### Inherited from BitComponentBase

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Class` | `string?` | `null` | CSS class name(s) to apply to the element. |
| `Style` | `string?` | `null` | Inline CSS styles to apply to the element. |
| `HtmlAttributes` | `Dictionary<string, object>` | `new()` | Additional HTML attributes to apply to the element. |
| `AriaLabel` | `string?` | `null` | Accessible label for assistive technologies. |
| `Dir` | `BitDir?` | `null` | Text directionality. Values: `Ltr` (Left-to-Right), `Rtl` (Right-to-Left), `Auto`. |
| `Id` | `string?` | `null` | Unique identifier for the element. |
| `TabIndex` | `string?` | `null` | Keyboard navigation order. |
| `Visibility` | `BitVisibility` | `Visible` | Display state of the element. Values: `Visible`, `Hidden`, `Collapsed`. |
| `IsEnabled` | `bool` | `true` | Component interaction state (enabled/disabled). |

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| `UniqueId` | `Guid` (read-only) | A unique identifier generated for each component instance. |
| `RootElement` | `ElementReference` | Reference to the root HTML element for direct DOM manipulation via JavaScript interop. |

### Code Examples

#### Basic Usage (Default Div)

```razor
<BitElement>
    This is default (div)
</BitElement>
```

#### Custom HTML Element

```razor
<BitElement Element="a" HtmlAttributes="new Dictionary<string, object> { { "href", "#" } }">
    Click me
</BitElement>
```

#### Dynamic Element with Styling

```razor
<BitElement Element="button" Class="custom-button" Style="padding: 10px; background-color: blue;">
    Submit
</BitElement>
```

#### Form Input Element

```razor
<BitElement Element="input" HtmlAttributes="new Dictionary<string, object> { { "type", "text" }, { "placeholder", "Enter text..." } }" />
```

#### With Accessibility

```razor
<BitElement AriaLabel="Main content section" Class="section" Dir="BitDir.Ltr">
    Accessible content here
</BitElement>
```

#### Progress Element

```razor
<BitElement Element="progress" HtmlAttributes="new Dictionary<string, object> { { "value", "70" }, { "max", "100" } }" />
```

#### Using RootElement Reference

```razor
<BitElement @ref="elementRef" Class="content-box">
    Content with element reference
</BitElement>

@code {
    private ElementReference elementRef;
    
    // Use elementRef for JavaScript interop or direct DOM manipulation
}
```

#### Conditional Visibility

```razor
<BitElement Visibility="showContent ? BitVisibility.Visible : BitVisibility.Hidden">
    This content is conditionally visible
</BitElement>

@code {
    private bool showContent = true;
}
```

---

## Icon

The **Icon** component represents concepts or meanings for users, enhancing user experience and creating more intuitive applications. It's part of the Bit BlazorUI utilities collection and provides a flexible way to display icons with customizable styling and sizing options.

### Component Parameters

#### Core Icon Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **IconName** | `string` | *required* | Specifies which icon to display from the Bit icon library |
| **Color** | `BitColor?` | `null` | Sets the icon's color scheme (see Color Options below) |
| **Size** | `BitSize?` | `null` | Controls icon dimensions (see Size Options below) |
| **Variant** | `BitVariant?` | `null` | Determines visual styling approach (see Variant Options below) |

#### Base Component Parameters (inherited from BitComponentBase)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **AriaLabel** | `string?` | `null` | Accessible label for assistive technologies |
| **Class** | `string?` | `null` | CSS class names for custom styling |
| **Dir** | `BitDir?` | `null` | Text directionality (Ltr, Rtl, Auto) |
| **Id** | `string?` | `null` | Unique HTML identifier |
| **IsEnabled** | `bool` | `true` | Enables/disables component interaction |
| **Style** | `string?` | `null` | Inline CSS styling |
| **TabIndex** | `string?` | `null` | Keyboard navigation order |
| **Visibility** | `BitVisibility` | `Visible` | Visual state control (see Visibility States below) |
| **HtmlAttributes** | `Dictionary<string, object>` | `null` | Additional HTML attributes |

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| **UniqueId** | `Guid` | Auto-generated component identifier |
| **RootElement** | `ElementReference` | Reference to root HTML element for JavaScript interop |

### Size Options (BitSize Enum)

| Value | Name |
|-------|------|
| `0` | **Small** |
| `1` | **Medium** |
| `2` | **Large** |

### Variant Options (BitVariant Enum)

| Value | Name |
|-------|------|
| `0` | **Fill** |
| `1` | **Outline** |
| `2` | **Text** |

### Color Options (BitColor Enum)

The component supports 16 color variants:

- **Primary**
- **Secondary**
- **Tertiary**
- **Info**
- **Success**
- **Warning**
- **SevereWarning**
- **Error**
- Background variants
- Foreground variants
- Border variants

### Visibility States (BitVisibility Enum)

| Value | Name | Description |
|-------|------|-------------|
| `0` | **Visible** | Standard display mode |
| `1` | **Hidden** | Space preserved (visibility: hidden) |
| `2` | **Collapsed** | Display: none (no space reserved) |

### Text Direction (BitDir Enum)

| Value | Description |
|-------|-------------|
| **Ltr** | Left-to-right languages |
| **Rtl** | Right-to-left languages |
| **Auto** | Browser-determined directionality |

### Icon Library

The component uses the **Bit Icon Library** which provides a comprehensive set of icon names available for the `IconName` parameter. Available icons include common UI concepts such as navigation, user interface, content actions, and more.

### Usage Notes

- The `IconName` parameter is required and determines which icon is displayed
- Size and color can be customized independently
- The variant option affects the visual presentation style
- Component is fully accessible with support for ARIA labels
- Integrates seamlessly with Bit's design system and theming

---

## Image

The BitImage component is a graphical representation utility for displaying images with support for lazy loading, fallback templates, and customizable styling.

### Description

An image is a graphic representation of something (e.g., photo or illustration). The BitImage component provides a feature-rich wrapper around standard image rendering with support for fade-in animations, lazy loading, error handling, and flexible sizing options.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Src` | `string?` | `null` | Image source URL |
| `Alt` | `string?` | `null` | Alternate text for accessibility |
| `Width` | `string?` | `null` | Image width value (CSS unit) |
| `Height` | `string?` | `null` | Image height value (CSS unit) |
| `Title` | `string?` | `null` | Tooltip text displayed on hover |
| `MaximizeFrame` | `bool` | `false` | Expands image to fill parent container |
| `FadeIn` | `bool` | `true` | Enables fade-in animation when image loads |
| `StartVisible` | `bool` | `true` | Shows image initially; hides if load fails |

### Image Fit Options

The component supports the `BitImageFit` enum to control how images scale within their frame:

- **None**: No scaling applied
- **Center**: Centered and cropped to fit frame
- **Contain**: Maintains aspect ratio, fully contained within frame
- **Cover**: Maintains aspect ratio, fills entire frame
- **CenterContain**: Centered positioning with aspect ratio preserved
- **CenterCover**: Centered fill with aspect ratio preserved

### Lazy Loading

The component supports the `BitImageLoading` enum with two loading strategies:

- **Eager** (default): Loads immediately when component renders
- **Lazy**: Defers loading until the image is needed or becomes visible

### State Management

The `BitImageState` enum tracks the image loading lifecycle:

- **Loading** (0): Image is currently loading
- **Loaded** (1): Image has successfully loaded
- **Error** (2): Image failed to load

### Fallback & Customization

- **LoadingTemplate**: Custom template displayed during the loading state
- **ErrorTemplate**: Custom template displayed when image fails to load
- **OnLoadingStateChange**: Callback event triggered when the image loading state changes

### Styling

Customize the component appearance via `BitImageClassStyles`:

- **Root element**: Apply styles and classes to the outer container
- **Image element**: Apply styles and classes to the `<img>` tag itself

### Code Examples

Basic image with source and alternative text:
```razor
<BitImage Src="image.jpg" Alt="Description of image" />
```

Image with custom dimensions and fit:
```razor
<BitImage Src="image.jpg" Alt="Description" Width="300px" Height="200px" ImageFit="BitImageFit.Cover" />
```

Image that fills its parent container:
```razor
<BitImage Src="image.jpg" Alt="Description" MaximizeFrame="true" />
```

Lazy-loaded image with fade-in animation:
```razor
<BitImage Src="image.jpg" Alt="Description" BitImageLoading="BitImageLoading.Lazy" FadeIn="true" />
```

Image with loading and error templates:
```razor
<BitImage Src="image.jpg" Alt="Description" OnLoadingStateChange="HandleStateChange">
    <LoadingTemplate>
        <p>Loading image...</p>
    </LoadingTemplate>
    <ErrorTemplate>
        <p>Failed to load image</p>
    </ErrorTemplate>
</BitImage>
```

Image with state tracking:
```razor
@code {
    private BitImageState imageState = BitImageState.Loading;
    
    private void HandleStateChange(BitImageState newState)
    {
        imageState = newState;
    }
}
```

---

The documentation has been extracted and formatted as a complete markdown section covering all aspects of the Bit.BlazorUI Image component, including its parameters, loading strategies, fallback options, and practical code examples.

---

## Label

The Label component provides a way to name or title controls and groups of controls, including text fields, checkboxes, combo boxes, radio buttons, and dropdown menus.

### Parameters

#### BitLabel-Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ChildContent | RenderFragment? | null | Content of the label; accepts custom tags or text |
| For | string? | null | Specifies which form element the label binds to |
| Required | bool | false | Indicates if associated field is required; displays a star (*) |

#### Inherited from BitComponentBase

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| AriaLabel | string? | null | Accessible label for assistive technologies |
| Class | string? | null | CSS class names to apply |
| Dir | BitDir? | null | Text directionality (Ltr, Rtl, Auto) |
| HtmlAttributes | Dictionary<string, object> | new Dictionary | Additional HTML attributes |
| Id | string? | null | Unique identifier for root element |
| IsEnabled | bool | true | Whether component responds to user interaction |
| Style | string? | null | CSS style string |
| TabIndex | string? | null | Keyboard navigation order |
| Visibility | BitVisibility | Visible | Display state (Visible, Hidden, Collapsed) |

### Public Members

- **UniqueId** (Guid): Read-only unique identifier assigned at construction
- **RootElement** (ElementReference): Reference to the root HTML element for DOM access

### Common Usage Patterns

- **Basic label rendering**: Display text labels for form controls
- **Visibility states**: Control display (Visible, Hidden, Collapsed)
- **Custom styling**: Apply CSS classes and inline styles
- **Required field indication**: Display asterisk for required fields using `Required="true"`
- **Label-input associations**: Use the `For` attribute to bind to associated form elements by ID
- **Internationalization**: Support for right-to-left (RTL) languages via the `Dir` parameter

---

## Link

The BitLink component represents an anchor element used for navigation. Links lead to another part of an app, other pages, or help articles. They can also be used to initiate commands.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| **ChildContent** | `RenderFragment?` | `null` | The inner content or text displayed within the link |
| **Href** | `string?` | `null` | The target URL for the link navigation |
| **Color** | `BitColor?` | `null` | Visual color variant for the link |
| **NoColor** | `bool` | `false` | Disables foreground coloring when set to true |
| **Underlined** | `bool` | `false` | Applies fixed underline styling to the link |
| **NoUnderline** | `bool` | `false` | Removes underline styling at all interactive states |
| **Target** | `string?` | `null` | Specifies how the link opens (see Target Options below) |
| **Rel** | `BitLinkRels?` | `null` | Defines the relationship to the linked document |
| **OnClick** | `EventCallback<MouseEventArgs>` | — | Event handler triggered when the link is clicked |
| **IsEnabled** | `bool` | `true` | Enables or disables the link (inherited from BitComponentBase) |
| **AriaLabel** | `string?` | `null` | Accessible label for screen readers |
| **Id** | `string?` | `null` | Unique identifier for the link element |
| **Class** | `string?` | `null` | Custom CSS class names |
| **Style** | `string?` | `null` | Inline CSS styles |
| **Dir** | `string?` | `null` | Text direction (for RTL support) |
| **TabIndex** | `int?` | `null` | Tab order for keyboard navigation |
| **Visibility** | `BitVisibility?` | `null` | Controls visibility of the link |
| **HtmlAttributes** | `Dictionary<string, object>?` | `null` | Additional HTML attributes |

### Color Options

- **Primary** (default)
- **Secondary**
- **Tertiary**
- **Info**
- **Success**
- **Warning**
- **SevereWarning**
- **Error**
- **Background** variants (Primary, Secondary, Tertiary)
- **Foreground** variants (Primary, Secondary, Tertiary)
- **Border** variants (Primary, Secondary, Tertiary)

### Target Options

| Value | Description |
|-------|-------------|
| `_self` | Opens link in the current context (default) |
| `_blank` | Opens link in a new tab or window |
| `_parent` | Opens link in the parent browsing context |
| `_top` | Opens link in the topmost browsing context |
| `_unfencedTop` | Opens link for fenced frames |

### Rel Attributes

The `Rel` parameter accepts `BitLinkRels` values to define relationships to the linked document:

- `NoFollow` - Search engines should not follow this link
- `NoOpener` - Opens without window.opener reference
- `NoReferrer` - No referrer information sent
- `External` - Marks link as external
- `Alternate` - Link is an alternate version
- `Author` - Link to author information
- `Bookmark` - Bookmarkable section
- `Help` - Link to help documentation
- `License` - Link to license information
- `Next` - Next page in a sequence
- `Prev` - Previous page in a sequence
- `Search` - Searchable link
- `Tag` - Associated tag

### Usage Examples

**Basic Link:**
```csharp
<BitLink Href="https://example.com">Click here</BitLink>
```

**Open in New Tab:**
```csharp
<BitLink Href="https://example.com" Target="_blank">External Link</BitLink>
```

**Styled Link with Color:**
```csharp
<BitLink Href="/products" Color="BitColor.Success">View Products</BitLink>
```

**Underlined Link:**
```csharp
<BitLink Href="/about" Underlined="true">About Us</BitLink>
```

**Link with Click Handler:**
```csharp
<BitLink OnClick="@HandleLinkClick">Trigger Action</BitLink>

@code {
    private async Task HandleLinkClick(MouseEventArgs args)
    {
        // Handle click event
    }
}
```

**Element Navigation (Hash Reference):**
```csharp
<BitLink Href="#section1">Jump to Section 1</BitLink>
```

**Disabled Link:**
```csharp
<BitLink Href="/dashboard" IsEnabled="false">Dashboard (Unavailable)</BitLink>
```

**With Rel Attributes:**
```csharp
<BitLink Href="https://external.com" Target="_blank" Rel="BitLinkRels.NoOpener | BitLinkRels.NoReferrer">
    Secure External Link
</BitLink>
```

**RTL Support:**
```csharp
<BitLink Href="/page" Dir="rtl">رابط عربي</BitLink>
```

---

## MediaQuery

**Component:** `BitMediaQuery`

A responsive utility component for Blazor that conditionally renders content based on CSS media query breakpoints. It allows developers to implement responsive behavior without custom media query logic.

### Component Description

BitMediaQuery is a Blazor utility component designed to implement responsive behavior using predefined CSS media queries. It enables conditional content rendering based on screen size breakpoints, supporting both predefined breakpoints and custom media queries.

### Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | Primary content rendered when query matches |
| `Matched` | `RenderFragment?` | `null` | Alias for ChildContent; content rendered when query matches |
| `NotMatched` | `RenderFragment?` | `null` | Content rendered when query doesn't match |
| `Query` | `string?` | `null` | Custom media query string (e.g., "screen and (max-width: 999px)") |
| `ScreenQuery` | `BitScreenQuery?` | `null` | Predefined screen breakpoint enum value |
| `OnChange` | `EventCallback<bool>` | - | Event triggered when query match state changes |

### Inherited Parameters

BitMediaQuery inherits from `BitComponentBase`:
- `Class` - CSS class names
- `Style` - Inline styles
- `Id` - HTML element identifier
- `AriaLabel` - ARIA label for accessibility
- `Dir` - Text direction (LTR/RTL)
- `HtmlAttributes` - Additional HTML attributes
- `IsEnabled` - Enable/disable state
- `TabIndex` - Tab index for keyboard navigation
- `Visibility` - Element visibility

### Responsive Breakpoints (BitScreenQuery Enum)

#### Exact Range Queries

| Breakpoint | Range | Media Query |
|-----------|-------|------------|
| `Xs` | ≤600px | `@media screen and (max-width: 600px)` |
| `Sm` | 601–960px | `@media screen and (min-width: 601px) and (max-width: 960px)` |
| `Md` | 961–1280px | `@media screen and (min-width: 961px) and (max-width: 1280px)` |
| `Lg` | 1281–1920px | `@media screen and (min-width: 1281px) and (max-width: 1920px)` |
| `Xl` | 1921–2560px | `@media screen and (min-width: 1921px) and (max-width: 2560px)` |
| `Xxl` | >2560px | `@media screen and (min-width: 2561px)` |

#### "Less Than" Range Queries

| Breakpoint | Meaning |
|-----------|---------|
| `LtSm` | Less than Small (≤960px) |
| `LtMd` | Less than Medium (≤1280px) |
| `LtLg` | Less than Large (≤1920px) |
| `LtXl` | Less than Extra Large (≤2560px) |
| `LtXxl` | Less than Double Extra Large (>2560px) |

#### "Greater Than" Range Queries

| Breakpoint | Meaning |
|-----------|---------|
| `GtXs` | Greater than Extra Small (>600px) |
| `GtSm` | Greater than Small (>960px) |
| `GtMd` | Greater than Medium (>1280px) |
| `GtLg` | Greater than Large (>1920px) |
| `GtXl` | Greater than Extra Large (>2560px) |

### Usage Examples

#### Basic Usage with Predefined Breakpoint

```razor
<BitMediaQuery ScreenQuery="BitScreenQuery.Md">
    <Matched>
        <p>This content appears on medium screens (961-1280px)</p>
    </Matched>
    <NotMatched>
        <p>This content appears on other screen sizes</p>
    </NotMatched>
</BitMediaQuery>
```

#### Using Range Queries

```razor
<!-- Shows on small and medium screens -->
<BitMediaQuery ScreenQuery="BitScreenQuery.LtLg">
    <ChildContent>
        <p>Visible on screens less than 1920px</p>
    </ChildContent>
</BitMediaQuery>

<!-- Shows on large screens and above -->
<BitMediaQuery ScreenQuery="BitScreenQuery.GtMd">
    <ChildContent>
        <p>Visible on screens greater than 1280px</p>
    </ChildContent>
</BitMediaQuery>
```

#### Custom Media Query

```razor
<BitMediaQuery Query="screen and (max-width: 999px)">
    <Matched>
        <p>This content appears when custom media query matches</p>
    </Matched>
</BitMediaQuery>
```

#### Handling State Changes

```razor
<BitMediaQuery ScreenQuery="BitScreenQuery.Md" OnChange="HandleMediaQueryChange">
    <ChildContent>
        <p>Responsive content</p>
    </ChildContent>
</BitMediaQuery>

@code {
    private void HandleMediaQueryChange(bool isMatched)
    {
        Console.WriteLine($"Media query matched: {isMatched}");
        // Perform additional logic when breakpoint changes
    }
}
```

### Key Features

- **Conditional Rendering:** Render different content based on screen size
- **Predefined Breakpoints:** Mobile-first responsive design with standard breakpoints
- **Custom Queries:** Support for any valid CSS media query string
- **Change Detection:** OnChange event for tracking breakpoint transitions
- **Accessibility:** Full ARIA label and semantic HTML support
- **Directional Support:** Built-in LTR/RTL text direction support
- **No Custom CSS:** Eliminates need to write custom media query logic

### Best Practices

1. Use predefined `BitScreenQuery` values for consistency across your application
2. Leverage range queries (`GtXx`, `LtXx`) for simpler responsive logic
3. Prefer the `Matched`/`NotMatched` pattern for clear conditional rendering
4. Use the `OnChange` event for non-visual responsive behaviors (e.g., analytics, data loading)
5. Combine multiple `BitMediaQuery` components for complex responsive layouts

---

## Overlay

### Component Description

The Overlay component is used to provide emphasis on a particular element or parts of it. It signals to the user of a state change within the application and can be used for creating loaders, dialogs and more.

### Parameters & Properties

#### BitOverlay-Specific Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `AutoToggleScroll` | `bool` | `false` | Disables scroll behavior of the Scroller element behind the overlay when enabled |
| `AbsolutePosition` | `bool` | `false` | Positions overlay as absolute instead of fixed |
| `ChildContent` | `RenderFragment?` | `null` | The overlay's content |
| `IsOpen` | `bool` | `false` | Controls overlay visibility |
| `NoAutoClose` | `bool` | `false` | Prevents automatic closing when clicking the overlay |
| `OnClick` | `EventCallback<MouseEventArgs>` | - | Callback for click interactions |
| `ScrollerSelector` | `string` | `"body"` | Selector for the element whose scroll will be disabled |

#### Inherited Properties (BitComponentBase)

- `AriaLabel` - Accessibility label
- `Class` - CSS class names
- `Dir` - Text directionality (BitDir enum: Ltr, Rtl, Auto)
- `HtmlAttributes` - Custom HTML attributes
- `Id` - Element identifier
- `IsEnabled` - Enable/disable state
- `Style` - Inline CSS styles
- `TabIndex` - Keyboard navigation order
- `Visibility` - Visibility state (BitVisibility enum: Visible, Hidden, Collapsed)

### Backdrop Options

The component supports two backdrop behaviors:

1. **Auto-Close Mode** (Default)
   - When `NoAutoClose` is `false`, clicking on the overlay backdrop automatically closes it
   - Useful for dialogs and temporary overlays

2. **Persistent Mode**
   - Set `NoAutoClose` to `true` to prevent automatic closing
   - Maintains overlay visibility even when backdrop is clicked
   - Suitable for loaders and persistent state indicators

### Scroll Control

- **AutoToggleScroll**: When enabled, disables scrolling on the element specified by `ScrollerSelector`
- **ScrollerSelector**: Defaults to `"body"` but can target any DOM element

### Positioning

- **AbsolutePosition**: Set to `true` for absolute positioning within a relative container; defaults to fixed positioning

### Code Examples

#### Basic Overlay with Auto-Close
```csharp
<BitOverlay IsOpen="@isOverlayOpen" />
```

#### Persistent Overlay with Custom Content
```csharp
<BitOverlay IsOpen="@isOverlayOpen" NoAutoClose="true">
    <div class="loader-content">Loading...</div>
</BitOverlay>
```

#### Overlay with Scroll Control
```csharp
<BitOverlay 
    IsOpen="@isOverlayOpen" 
    AutoToggleScroll="true"
    ScrollerSelector="body" />
```

#### Absolute Positioned Overlay
```csharp
<div style="position: relative;">
    <BitOverlay 
        IsOpen="@isOverlayOpen" 
        AbsolutePosition="true" />
</div>
```

#### Overlay with Click Handler
```csharp
<BitOverlay 
    IsOpen="@isOverlayOpen"
    OnClick="@HandleOverlayClick" />

@code {
    private async Task HandleOverlayClick(MouseEventArgs e)
    {
        // Custom click handling
    }
}
```

---

---

## PullToRefresh

### Description

The PullToRefresh component implements a "pull down to refresh" feature commonly found in mobile applications and modern web interfaces. It detects downward user gestures on a page or specific container element and triggers refresh callbacks when configured thresholds are met. This component is ideal for scenarios where users need to manually refresh content by pulling down, providing intuitive gesture-based interaction patterns.

### Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Anchor` / `ChildContent` | `RenderFragment?` | `null` | Container element for the component content that can be refreshed |
| `Factor` | `decimal` | `2` | Balancing multiplier for pull height, controlling resistance during pulling |
| `Trigger` | `int` | `80` | Pull distance in pixels that initiates the refresh callback |
| `Threshold` | `int` | `0` | Minimum pull distance in pixels before processing pull gestures |
| `Margin` | `int` | `30` | Top margin in pixels for the pull element positioning |
| `ScrollerElement` | `ElementReference?` | `null` | Reference to a specific scrollable container element for pull-to-refresh detection |
| `ScrollerSelector` | `string?` | `null` | CSS selector string to identify the scrollable container element |
| `Loading` | `RenderFragment?` | `null` | Custom loading indicator template to display during refresh operations |
| `Class` | `string?` | `null` | Additional CSS classes applied to the root element |
| `Style` | `string?` | `null` | Inline CSS styles applied to the root element |
| `Id` | `string?` | `null` | HTML id attribute for the component |
| `AriaLabel` | `string?` | `null` | ARIA label for accessibility |
| `Dir` | `string?` | `null` | Text direction (ltr/rtl) |
| `TabIndex` | `int?` | `null` | Tab order index |
| `IsEnabled` | `bool` | `true` | Enables or disables the pull-to-refresh functionality |
| `Visibility` | `string?` | `null` | Controls component visibility |
| `HtmlAttributes` | `Dictionary<string, object>?` | `null` | Additional HTML attributes to apply to the component |

### Event Handlers

- **`OnRefresh`** - Triggered when the pull distance exceeds the configured `Trigger` value. This is the primary event where refresh logic should be implemented.

- **`OnPullStart`** - Fires when a pull gesture begins. Provides position data including `Top`, `Left`, and `Width` properties for advanced positioning logic.

- **`OnPullMove`** - Fires continuously during the pull gesture. Returns the current pull distance allowing for dynamic progress indicators or visual feedback.

- **`OnPullEnd`** - Fires when the pull gesture completes or is cancelled. Returns the final pull distance before gesture termination.

### Styling & Customization

The component supports style customization through the **`BitPullToRefreshClassStyles`** object which allows customization of:

- **Root container** - Main wrapper element styles
- **Loading indicator** - Container for the loading visual
- **Spinner wrapper** - Loading spinner container (supports default and refreshing states)
- **Spinner element** - Animated spinner visual (supports default and refreshing states)

Custom loading indicators can be provided through the `Loading` render fragment parameter to replace the default loading visual.

### Basic Usage Example

```razor
<BitPullToRefresh OnRefresh="@OnRefreshAsync" Trigger="80">
    <div>
        <!-- Your scrollable content here -->
        @foreach(var item in Items)
        {
            <div>@item.Title</div>
        }
    </div>
</BitPullToRefresh>

@code {
    private List<string> Items { get; set; } = new();

    private async Task OnRefreshAsync()
    {
        // Perform refresh operation (API call, data reload, etc.)
        Items = await LoadDataAsync();
    }

    private async Task<List<string>> LoadDataAsync()
    {
        // Simulated data loading
        await Task.Delay(1000);
        return new List<string> { "Item 1", "Item 2", "Item 3" };
    }
}
```

### Advanced Features

- **Multiple Instances** - Multiple PullToRefresh components can be used on the same page with independent refresh operations
- **Custom Scroller** - Specify a specific scrollable container using `ScrollerElement` reference or `ScrollerSelector` CSS selector rather than the entire page
- **Pull Gesture Events** - Access granular pull events (`OnPullStart`, `OnPullMove`, `OnPullEnd`) for creating custom progress indicators or advanced UX patterns
- **Mobile Optimized** - Designed specifically for mobile app scenarios while remaining functional on desktop browsers
- **Accessibility** - Inherits from `BitComponentBase` with full support for ARIA labels and keyboard navigation

### Configuration Notes

- The `Factor` parameter (default `2`) controls how much resistance users feel while pulling - higher values mean stronger resistance
- The `Trigger` threshold is the minimum pull distance needed to activate a refresh operation
- The `Threshold` parameter can be set to require an initial pull distance before any pull events are fired
- Use `ScrollerSelector` with CSS selectors like `.content-area` or `#main-content` to target specific scrollable regions

---

This documentation provides a comprehensive reference for implementing the PullToRefresh component in Bit.BlazorUI applications.

---

## Separator

The **Separator** component from Bit.BlazorUI is a utility component designed to visually separate content into groups, providing clear visual organization within your Blazor applications.

### Component Description

The Separator component is a flexible content divider that supports both horizontal and vertical orientations. It can display text, icons, or custom content within the separator, making it suitable for various UI layouts and organizational needs.

### Core Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Vertical` | `bool` | `false` | Toggles between horizontal (false) and vertical (true) orientation |
| `AlignContent` | `BitSeparatorAlignContent` | `Center` | Positions the alignment of content within the separator |
| `AutoSize` | `bool` | `false` | Enables automatic width/height rendering based on content |
| `Background` | `BitColorKind?` | `null` | Sets the background color styling using predefined color kinds |
| `Border` | `BitColorKind?` | `null` | Sets the border color styling using predefined color kinds |
| `ChildContent` | `RenderFragment?` | `null` | Accepts custom tags or text content to be rendered within the separator |

### Inherited Base Parameters

The component inherits additional parameters from `BitComponentBase`:
- `AriaLabel` - Accessibility label for the component
- `Class` - Custom CSS class names
- `Dir` - Text direction (LTR/RTL)
- `HtmlAttributes` - Additional HTML attributes
- `Id` - Element identifier
- `IsEnabled` - Enable/disable state
- `Style` - Inline CSS styles
- `TabIndex` - Tab order index
- `Visibility` - Control component visibility

### Orientation Options

**Horizontal (Default)**
- Content is arranged left to right
- Set `Vertical="false"` or omit the parameter
- Ideal for separating sections or groups horizontally

**Vertical**
- Content is arranged top to bottom
- Set `Vertical="true"`
- Useful for vertical layout separations

### Content Alignment

The `AlignContent` parameter uses the `BitSeparatorAlignContent` enum with three options:

| Alignment | Value | Description |
|-----------|-------|-------------|
| `Start` | 0 | Aligns content to the start of the separator |
| `Center` | 1 | Centers content within the separator (default) |
| `End` | 2 | Aligns content to the end of the separator |

### Color Customization

The `Background` and `Border` parameters accept `BitColorKind` enum values for theme-based styling:

- **Primary** - Primary theme color
- **Secondary** - Secondary theme color
- **Tertiary** - Tertiary theme color
- **Transparent** - Transparent/no color

### Key Features

- **Text and Icon Support** - Display text, icons, or custom content within separators
- **FullWidth Option** - Works with flex containers for full-width layouts
- **Multiple Color Themes** - Customizable colors for both background and border
- **Accessibility Support** - Built-in support for accessibility via `AriaLabel` parameter
- **Flexible Orientation** - Switch between horizontal and vertical layouts as needed

### Basic Usage Example

```blazor
<!-- Horizontal Separator (Default) -->
<BitSeparator />

<!-- Horizontal Separator with Text -->
<BitSeparator ChildContent="@((__builder) => {
    @(__builder.AddContent(0, "or"));
})" />

<!-- Vertical Separator -->
<BitSeparator Vertical="true" />

<!-- Separator with Custom Alignment -->
<BitSeparator AlignContent="BitSeparatorAlignContent.Start" />

<!-- Separator with Color Customization -->
<BitSeparator Background="BitColorKind.Primary" Border="BitColorKind.Secondary" />

<!-- Separator with Auto-sizing -->
<BitSeparator AutoSize="true" />

<!-- Vertical Separator with Content and Custom Styling -->
<BitSeparator Vertical="true" 
              AlignContent="BitSeparatorAlignContent.Center"
              ChildContent="@((__builder) => {
                  @(__builder.AddContent(0, "or"));
              })" />
```

### Repository

The Bit.BlazorUI Separator component is part of the open-source BitPlatform project and is available on GitHub at [bitfoundation/bitplatform](https://github.com/bitfoundation/bitplatform) for review and contribution.

---

## Sticky

The Sticky component enables elements to stick during scrolling. It allows developers to create fixed positioning behavior for content within scrollable containers across vertical and horizontal axes.

### Component Parameters

#### BitSticky-Specific Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Top` | `string?` | `null` | Sets vertical distance from top edge (e.g., "20px", "2rem") |
| `Bottom` | `string?` | `null` | Sets vertical distance from bottom edge |
| `Left` | `string?` | `null` | Sets horizontal distance from left edge |
| `Right` | `string?` | `null` | Sets horizontal distance from right edge |
| `StickyPosition` | `BitStickyPosition` | `Top` | Defines region where sticky behavior applies (see positioning modes below) |
| `ChildContent` | `RenderFragment?` | `null` | Container for component content (text/custom tags) |

#### Inherited Parameters (BitComponentBase)

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `Class` | `string?` | `null` | CSS class names |
| `Style` | `string?` | `null` | Inline CSS styling |
| `Id` | `string?` | `null` | Unique element identifier |
| `AriaLabel` | `string?` | `null` | Accessibility label for assistive technologies |
| `Dir` | `BitDir?` | `null` | Text directionality (Ltr/Rtl/Auto) |
| `IsEnabled` | `bool` | `true` | Enables user interaction capability |
| `TabIndex` | `string?` | `null` | Keyboard navigation order |
| `Visibility` | `BitVisibility` | `Visible` | Display state (Visible/Hidden/Collapsed) |
| `HtmlAttributes` | `Dictionary` | Empty | Additional HTML attributes |

### Sticky Positioning Modes

The **BitStickyPosition** enum supports the following positioning behaviors:

- **Top** (0): Sticks to top edge when scrolling
- **Bottom** (1): Sticks to bottom edge when scrolling
- **TopAndBottom** (2): Adheres to both top and bottom edges
- **Start** (3): Sticks to left edge (LTR) or right edge (RTL, respects directionality)
- **End** (4): Sticks to right edge (LTR) or left edge (RTL, respects directionality)
- **StartAndEnd** (5): Adheres to both horizontal edges

### Public Members

| Member | Type | Description |
|--------|------|-------------|
| `UniqueId` | `Guid` | Auto-generated unique identifier for the component instance |
| `RootElement` | `ElementReference` | Reference to root HTML element for DOM manipulation |

### Usage Examples

The component supports various positioning scenarios:

- **Basic vertical sticky**: Top positioning with custom spacing
- **Vertical combinations**: Top and bottom positioning simultaneously
- **Horizontal positioning**: Start and end positioning with RTL support
- **Custom spacing**: Supports pixel units ("20px") and rem units ("2rem")
- **Scrollable containers**: Works within scrollable parent elements with content overflow

All positioning examples include interactive demonstrations where content scrolls while the sticky element maintains its position relative to the specified edges.

---

## SwipeTrap

### Component Description

The **SwipeTrap** is a utility component in Bit.BlazorUI that "traps swipe actions and triggers corresponding events." It enables developers to detect and respond to touch-based swipe gestures on mobile and desktop interfaces. The component wraps content and detects swipe movements in all directions, firing events at different stages of the swipe lifecycle.

### Core Parameters & Properties

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment` | Optional | Renders the wrapped content that will respond to swipe gestures |
| `OnStart` | `EventCallback<BitSwipeTrapEventArgs>` | - | Fires when swiping begins on the container |
| `OnMove` | `EventCallback<BitSwipeTrapEventArgs>` | - | Fires during active swiping movement |
| `OnEnd` | `EventCallback<BitSwipeTrapEventArgs>` | - | Fires when the swipe action completes |
| `OnTrigger` | `EventCallback<BitSwipeTrapTriggerArgs>` | - | Fires when swiping exceeds configured thresholds |
| `OrientationLock` | `BitSwipeOrientation` | None | Restricts detection to horizontal/vertical directions (optimizes performance) |
| `Threshold` | `decimal` | - | Minimum pixel distance required to initiate swipe detection |
| `Throttle` | `int` | ~10ms | Delay in milliseconds between event emissions |
| `Trigger` | `decimal` | 0.25 (25%) | Swiping distance fraction that activates the OnTrigger event |

### Event Arguments

#### BitSwipeTrapEventArgs
Used by `OnStart`, `OnMove`, and `OnEnd` events:
- **StartX** (`double`): Initial horizontal touch coordinate
- **StartY** (`double`): Initial vertical touch coordinate  
- **DiffX** (`double`): Horizontal displacement in pixels from start point
- **DiffY** (`double`): Vertical displacement in pixels from start point

#### BitSwipeTrapTriggerArgs
Used by `OnTrigger` event:
- **Direction** (`BitSwipeDirection`): Enumerated swipe direction (Right, Left, Top, Bottom)
- **DiffX** (`double`): Horizontal displacement in pixels
- **DiffY** (`double`): Vertical displacement in pixels

### Swipe Gesture Handling

The component supports four primary swipe directions:
- **Right**: Swipe movement from left to right
- **Left**: Swipe movement from right to left
- **Top**: Swipe movement from bottom to top
- **Bottom**: Swipe movement from top to bottom

**Orientation Locking**: Use the `OrientationLock` property to constrain swipe detection to specific directions (Horizontal or Vertical), which optimizes performance for use cases where only certain directions matter.

**Threshold & Trigger**: 
- `Threshold` defines the minimum distance required to register a swipe
- `Trigger` defines the distance threshold (as a fraction, default 25%) at which the `OnTrigger` event fires
- `Throttle` prevents event spam by delaying emissions between calls

### Practical Applications

The SwipeTrap component is designed for:
1. **Panel Controls** - Swipe to open/close panels (e.g., swipe left to close)
2. **List Operations** - Swipe list items to trigger actions like delete (e.g., swipe right to delete)
3. **Mobile Navigation** - Left/right menu integration with swipe gestures
4. **Touch Interactions** - General touch-based gesture handling for enhanced user experience

### Implementation Considerations

- Works on both mobile (touch) and desktop (mouse drag) interfaces
- Configurable throttling prevents performance issues with high-frequency events
- OrientationLock optimization allows detection of only needed directions
- Multiple event stages (Start, Move, End, Trigger) provide granular control over gesture response

---

**Source**: [Bit.BlazorUI SwipeTrap Component](https://blazorui.bitplatform.dev/components/swipetrap)

---

## Text

### Component Description

The BitText component is a utility component designed to "present your design and content as clearly and efficiently as possible." It provides flexible text rendering with multiple styling options, typography presets, and color customization capabilities.

### Parameters & Properties

#### Core BitText Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `ChildContent` | `RenderFragment?` | `null` | The text content to display within the component |
| `Color` | `BitColor?` | `null` | General color scheme applied to the text |
| `Element` | `string?` | `null` | Custom HTML element to use for the root node |
| `ForceBreak` | `bool` | `false` | Forces text to break at the end of content boundaries |
| `Foreground` | `BitColorKind?` | `null` | Foreground color styling (Primary, Secondary, Tertiary, Info, Success, Warning, SevereWarning, Error) |
| `Gutter` | `bool` | `false` | Adds bottom margin spacing when enabled |
| `NoWrap` | `bool` | `false` | Truncates overflowing text with ellipsis instead of wrapping |
| `Typography` | `BitTypography?` | `null` | Text styling and hierarchy preset |

#### Inherited Parameters (from BitComponentBase)

Standard accessibility and styling properties: `AriaLabel`, `Class`, `Dir`, `HtmlAttributes`, `Id`, `IsEnabled`, `Style`, `TabIndex`, `Visibility`, `UniqueId`

### Typography Options

The component supports 14 typography styles for text hierarchy and styling:

- **Headings**: H1, H2, H3, H4, H5, H6
- **Subtitles**: Subtitle1, Subtitle2
- **Body Text**: Body1, Body2
- **Button**: Button
- **Captions**: Caption1, Caption2
- **Other**: Overline, Inherit

### Text Wrapping Modes

The component provides three text wrapping configurations:

1. **Normal Wrap** (default): Standard text wrapping behavior allowing content to flow naturally
2. **NoWrap**: Truncates overflowing text with an ellipsis (`...`) instead of wrapping
3. **ForceBreak**: Forces line breaks at content boundaries

### Alignment Options

Text can be aligned using the following alignment modes:
- **Start**: Left-aligned (default)
- **Center**: Center-aligned
- **End**: Right-aligned

### Color Support

The component supports multiple color kinds for text styling:
- **Primary, Secondary, Tertiary**: Semantic color options
- **Status Colors**: Info, Success, Warning, SevereWarning, Error
- **Variants**: Background and border color variations via the `Foreground` parameter

### Usage Examples

```csharp
// Basic text with typography
<BitText Typography="BitTypography.H1">
    Welcome to My Application
</BitText>

// Body text with color
<BitText Typography="BitTypography.Body1" Foreground="BitColorKind.Primary">
    This is primary colored body text.
</BitText>

// Subtitle with bottom margin (gutter)
<BitText Typography="BitTypography.Subtitle1" Gutter="true">
    Section Subtitle
</BitText>

// Text with no wrapping (truncated with ellipsis)
<BitText Typography="BitTypography.Body2" NoWrap="true">
    This is a very long text that will be truncated with an ellipsis if it exceeds the container width
</BitText>

// Caption with forced line breaks
<BitText Typography="BitTypography.Caption1" ForceBreak="true">
    Text with forced breaks at boundaries
</BitText>

// Custom styling and classes
<BitText Typography="BitTypography.Body1" Class="custom-class" Style="margin: 10px;">
    Styled text content
</BitText>

// Error status text
<BitText Typography="BitTypography.Body2" Foreground="BitColorKind.Error">
    An error occurred during processing.
</BitText>
```

---

# Extras

## Iconography

### Overview
Bit.BlazorUI provides a comprehensive iconography system using a custom font derived from the **MDL2 design system (Office UI Fabric icons)**. The icon system supports scaling, coloring, and custom styling to fit any design requirements.

### Installation
To use icons in your Bit.BlazorUI project, you must install the required NuGet package:

```
Bit.BlazorUI.Icons
```

This package should be added to your project during setup, as documented in the Getting Started section.

### Available Icon Sets
The Bit.BlazorUI iconography system includes an extensive library of **over 1,400-2,000+ predefined icon names**, organized into the following categories:

- **Navigation Icons**: Back, NavigateForward, ChevronDown, ChevronUp, etc.
- **Document/File Icons**: FileCode, FileHTML, ExcelDocument, Word, PDF, etc.
- **UI Control Icons**: Checkbox, RadioBtnOn, Toggle, Settings, etc.
- **Status & Alert Icons**: StatusCircleCheckmark, Warning, ErrorBadge, InfoIcon, etc.
- **Brand & Logo Icons**: TeamsLogo, PowerPointLogo, SharePointLogo, SkypeForBusinessLogo, PowerBILogo, etc.
- **Weather & Nature Icons**: Sunny, RainShowersDay, Snowflake, CloudWeather, etc.
- **Common Actions**: Add, Delete, Search, Edit, Copy, Paste, etc.

### Icon Catalog & Discovery
The Bit.BlazorUI documentation provides an interactive icon catalog with the following features:

- **1,400-2,000+ named icons** available for use in components
- **Interactive browsing**: Browse and preview all available icons
- **Clipboard copying**: Click on any icon to automatically copy its name to your clipboard, streamlining development workflow
- **Searchable interface**: Find icons by category or name

### Using Icons in Components
The custom font-based icon system allows you to:

- Scale icons to any size
- Apply custom colors
- Style icons with CSS to match your design system
- Reference icons by their registered names in your Blazor components

### Icon Font Characteristics
- **Source**: MDL2 design system (Office UI Fabric icons)
- **Format**: Custom font
- **Flexibility**: Full support for CSS scaling and color customization
- **Integration**: Seamlessly integrates with Bit.BlazorUI components

### Getting Started
For detailed code examples and implementation patterns, refer to:
1. The official Bit.BlazorUI documentation at https://blazorui.bitplatform.dev/iconography
2. The GitHub repository referenced in the documentation for practical implementation examples

---

## Theming

### Overview

Bit.BlazorUI provides a comprehensive theming system built on Microsoft's Fluent design system. It allows complete customization of component appearance through CSS variables, color schemes, and theme providers with support for light and dark mode options.

### Quick Setup

To enable theming in your Blazor application:

**1. Register Services (Program.cs)**
```csharp
builder.Services.AddBitBlazorUIServices();
```

**2. Add Theme System to HTML**
```html
<html bit-theme-system>
  <!-- your content -->
</html>
```

The `bit-theme-system` attribute automatically follows system theme preferences.

### CSS Variables

Theme values are applied to the root element using prefixed CSS variables. These can be overridden to customize the UI appearance:

**Common Color Variables:**
- `--bit-clr-pri` - Primary color
- `--bit-clr-bg-pri` - Primary background color
- `--bit-clr-fg-sec` - Secondary foreground color
- `--bit-clr-bg-sec` - Secondary background color
- `--bit-clr-border-sec` - Secondary border color

**Custom Override Example:**
```css
:root {
  --bit-clr-pri: #0078d4;
  --bit-clr-bg-pri: #ffffff;
  --bit-clr-fg-sec: #333333;
}
```

You can simply override these CSS variables to customize the UI appearance throughout your application.

### Dark/Light Mode Implementation

#### HTML Tag Attributes

Control theme behavior with attributes on the `<html>` tag:

- **`bit-theme-system`** - Automatically follow OS/system theme preferences
- **`bit-theme-persist`** - Save selected theme to local storage for persistence
- **`bit-theme-default="dark"`** - Set the default theme (e.g., "dark" or "light")
- **`bit-theme-dark="custom-dark"`** - Customize theme names

**Example:**
```html
<html bit-theme-system bit-theme-persist bit-theme-default="light">
```

### Theme Customization Methods

#### BitThemeManager (C# API)

The `BitThemeManager` service provides programmatic control over theming:

```csharp
// Apply custom theme values
await bitThemeManager.ApplyBitThemeAsync();

// Switch to a named theme
await bitThemeManager.SetThemeAsync("dark");

// Toggle between dark and light modes
await bitThemeManager.ToggleDarkLightAsync();

// Get current theme
var currentTheme = await bitThemeManager.GetCurrentThemeAsync();
```

#### BitThemeProvider Component

Wrap elements with `BitThemeProvider` to apply scoped theme customization:

```razor
<BitThemeProvider>
  <!-- Elements inside will use the custom theme -->
  <div>
    <BitButton>Themed Button</BitButton>
  </div>
</BitThemeProvider>
```

Supports nesting for granular styling control at different levels of your component hierarchy.

#### BitCss Class (C# Type-Safe Access)

Access CSS classes and variables in C# code for type-safe theming:

```csharp
// Access CSS classes
BitCss.Class.Color.Background.Primary.Main

// Access CSS variables
BitCss.Var.Color.Border.Secondary.Main
```

### JavaScript API

For client-side theme manipulation in JavaScript/TypeScript:

```javascript
// Get current theme
const theme = BitTheme.get();

// Set theme
BitTheme.set('dark');

// Toggle between dark and light modes
BitTheme.toggleDarkLight();

// Listen for theme changes
BitTheme.onChange((newTheme) => {
  console.log('Theme changed to:', newTheme);
});

// Initialize with options
BitTheme.init({ /* options */ });
```

### Summary

Bit.BlazorUI's theming system provides multiple layers of customization:
- **CSS Variables** for styling customization at the stylesheet level
- **HTML Attributes** for declarative theme behavior
- **C# APIs** for programmatic theme control in Blazor components
- **JavaScript APIs** for client-side theme management
- **BitThemeProvider** component for scoped theme application
- **Type-safe CSS access** through the BitCss class

This flexible approach allows you to implement consistent, maintainable theming across your entire Blazor application while supporting dark mode, light mode, and system preference detection.

---

*End of Bit.BlazorUI Component Reference*
