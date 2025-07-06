$(document).ready(function () {
    // 初始化紅點
    updateCartCount();

    // 加入購物車按鈕點擊事件
    $('.add-to-cart-btn').click(function (e) {
        e.preventDefault();
        const productId = $(this).data('product-id');

        $.ajax({
            type: 'POST',
            url: '/Cart/Add',
            data: { productId: productId, quantity: 1 },
            success: function () {
                updateCartCount(); // 更新紅點
                // ✅ 顯示成功提示
                const alertBox = $('#cart-alert');
                alertBox.stop(true, true).fadeIn().delay(300).fadeOut();
            },
            error: function () {
                alert("加入購物車失敗！");
            }
        });
    });

    // 封裝紅點更新函式
    function updateCartCount() {
        $.get('/Cart/GetCartItemCount', function (data) {
            const badge = $('#cart-count-badge');
            if (data.count > 0) {
                badge.text(data.count);
                badge.show();
            } else {
                badge.hide();
            }
        });
    }
});

$(document).ready(function () {
    // 🔁 確保紅點一直更新
    updateCartCount();

    // ⭐️ 移除按鈕點擊處理
    $('.remove-cart-item').click(function (e) {
        e.preventDefault();

        const cartItemId = $(this).data('cart-item-id');
        const rowId = $(this).data('item-row-id');

        $.ajax({
            type: 'POST',
            url: '/Cart/RemoveFromCart',
            data: { cartItemId: cartItemId },
            success: function () {
                $('#' + rowId).fadeOut(300, function () {
                    $(this).remove();

                    // ✅ 更新總金額與紅點
                    updateMainTotal();
                    updateCartCount();

                    // ✅ 如果整個購物車為空，顯示「購物車是空的」提示
                    if ($('.main-subtotal').length === 0) {
                        $('table.table').remove(); // 移除表格
                        $('.d-flex.gap-2').remove(); // 移除按鈕區塊
                        $('h2').after(`
                <div class="alert alert-info">購物車是空的</div>
                <div class="d-flex gap-2">
                    <a class="btn btn-secondary" href="/">🔙 繼續購物</a>
                </div>
            `);
                    }
                });

                // 顯示移除提示
                const alertBox = $('#cart-remove-alert');
                alertBox.stop(true, true).fadeIn().delay(200).fadeOut();
            },
            error: function () {
                alert("移除失敗！");
            }
        });
    });

    function updateCartCount() {
        $.get('/Cart/GetCartItemCount', function (data) {
            const badge = $('#cart-count-badge');
            if (data.count > 0) {
                badge.text(data.count);
                badge.show();
            } else {
                badge.hide();
            }
        });
    }
});
//頁尾按鈕控制
document.addEventListener('DOMContentLoaded', function () {
    const toggleBtn = document.getElementById('toggleFooterBtn');
    const footerCollapse = document.getElementById('footerDetails');

    footerCollapse.addEventListener('shown.bs.collapse', () => {
        toggleBtn.textContent = '收起詳細資訊';
    });

    footerCollapse.addEventListener('hidden.bs.collapse', () => {
        toggleBtn.textContent = '展開詳細資訊';
    });
});



