$(document).ready(function () {
    loadPageCartItems();
    updateCartCount();

    // Update cart quantities
    $('.update-cart-btn').click(function () {
        updateCartQuantities();
        new swal("Cart Updated!", "Shopping cart updated successfully.", "success");
    });

    // Quantity adjustment buttons
    $(document).on('click', '.btn-num-product-up', function () {
        let quantityInput = $(this).siblings('.num-product');
        quantityInput.val(parseInt(quantityInput.val()) + 1);
    });

    $(document).on('click', '.btn-num-product-down', function () {
        let quantityInput = $(this).siblings('.num-product');
        if (parseInt(quantityInput.val()) > 1) {
            quantityInput.val(parseInt(quantityInput.val()) - 1);
        }
    });

    // Remove item
    $(document).on('click', '.remove-item-btn', function () {
        let itemId = $(this).data('item-id');
        removeFromCart(itemId);
    });

    // Checkout button
    $("#buyNowBtn").click(function () {
        handleCheckout();
    });
});

// Load cart items dynamically
function loadPageCartItems() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    let cartItemsContainer = $('.header-cart-wrapitem');
    cartItemsContainer.empty();

    if (!cart.length) {
        cartItemsContainer.append('<tr><td colspan="4" class="text-center">Your cart is empty.</td></tr>');
        return;
    }

    let itemCountMap = cart.reduce((acc, itemId) => {
        acc[itemId] = (acc[itemId] || 0) + 1;
        return acc;
    }, {});

    let itemIds = Object.keys(itemCountMap);
    Promise.all(itemIds.map(itemId => fetch(`/api/home/Book/${itemId}`).then(res => res.json())))
        .then(results => {
            results.forEach((item, index) => {
                if (!item.bookId) {
                    console.error(`Failed to load item with ID ${itemIds[index]}`);
                    return;
                }

                let quantity = itemCountMap[itemIds[index]];
                let imageUrl = item.imageUrl ? `data:image/jpeg;base64,${item.imageUrl}` : '/images/default.jpg';

                cartItemsContainer.append(`
                    <tr class="table_row" style="border: 1px solid #ddd; text-align: center;">
                        <td class="column-1" style="border: 1px solid #ddd; padding: 10px;">
                            <div class="align-items-center" style="gap: 10px;">
                                <img src="${imageUrl}" alt="${item.name}" style="width: 60px; height: auto; border-radius: 5px;">
                                <span style="font-size: 0.9rem; color: #333; font-weight: bold;">${item.name}</span>
                            </div>
                        </td>
                        <td class="column-2" style="border: 1px solid #ddd; padding: 10px; text-align: center;">
                            <div class="d-flex align-items-center justify-content-center" style="gap: 5px;">
                                <button class="btn btn-light btn-num-product-down me-2" style="border: 1px solid #ccc;">-</button>
                                <input class="form-control num-product text-center" type="number" value="${quantity}" style="width: 60px; border: 1px solid #ccc;">
                                <button class="btn btn-light btn-num-product-up ms-2" style="border: 1px solid #ccc;">+</button>
                            </div>
                        </td>
                        <td class="column-3 " style="border: 1px solid #ddd; padding: 10px;">
                            <button class="btn btn-danger remove-item-btn" data-item-id="${item.bookId}" 
                                style="border: 1px solid #ccc; border-radius: 3px;">Remove</button>
                        </td>
                    </tr>
                `);

            });
        })
        .catch(error => {
            console.error("Error loading cart items:", error);
            new swal("Error", "Failed to load cart items. Please try again.", "error");
        });
}

// Update quantities in localStorage
function updateCartQuantities() {
    let cart = [];
    $('.table-shopping-cart tbody tr').each(function () {
        let itemId = $(this).find('.remove-item-btn').data('item-id');
        let updatedQty = parseInt($(this).find('.num-product').val());
        for (let i = 0; i < updatedQty; i++) {
            cart.push(itemId);
        }
    });
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartCount();
}

// Update cart item count
function updateCartCount() {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    $('.icon-header-noti').attr('data-notify', cart.length);
}

// Remove an item
function removeFromCart(itemId) {
    let cart = JSON.parse(localStorage.getItem('cart')) || [];
    cart = cart.filter(id => id !== itemId);
    localStorage.setItem('cart', JSON.stringify(cart));
    loadPageCartItems();
    updateCartCount();
    new swal("Item Removed!", "Item has been removed from your cart.", "success");
}

// Handle checkout
function handleCheckout() {
    let userId = parseInt($("#buyNowBtn").data("user-id"));

    if (!userId) {
        new swal("Login Required!", "Please login or signup to continue.", "warning").then(() => {
            window.location.href = '/Home/LoginPage';
        });
        return;
    }

    const cartItems = JSON.parse(localStorage.getItem("cart")) || [];
    if (!cartItems.length) {
        new swal("Cart Empty!", "Your cart is empty. Add items to proceed.", "warning");
        return;
    }


    const itemCounts = cartItems.reduce((acc, itemId) => {
        acc[itemId] = (acc[itemId] || 0) + 1;
        return acc;
    }, {});

    const itemIds = Object.keys(itemCounts);

    
    Promise.all(itemIds.map(itemId => fetch(`/api/home/Book/${itemId}`).then(res => res.json())))
        .then(results => {
            let totalAmount = 0;
            const lineItems = [];

            results.forEach((item, index) => {
                if (!item.bookId) {
                    console.error(`Failed to load item with ID ${itemIds[index]}`);
                    return;
                }

                const quantity = itemCounts[itemIds[index]];
                const totalPrice = item.price * quantity;
                totalAmount += totalPrice;

                lineItems.push({
                    OrderId: 0,
                    BookId: parseInt(item.bookId),
                    Quantity: quantity,
                    TotalPrice: totalPrice
                });
            });

            const order = {
                UserId: userId,
                TotalAmount: totalAmount,
                Status: "Pending"
            };

            return fetch('/api/home/CreateOrder', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(order)
            }).then(res => res.json())
                .then(data => {
                    if (data.success) {
                        
                        lineItems.forEach(item => item.orderId = data.orderId);

                        
                        return fetch('/api/home/CreateOrderLineItems', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify(lineItems)
                        });
                    } else {
                        throw new Error("Failed to create order");
                    }
                });
        })
        .then(() => {
            localStorage.removeItem('cart');
            updateCartCount();
            new swal("Order Placed!", "Your order has been successfully placed.", "success");
        })
        .catch(error => {
            console.error('Error during checkout:', error);
            new swal("Error", "Failed to process checkout. Please try again.", "error");
        });
}

